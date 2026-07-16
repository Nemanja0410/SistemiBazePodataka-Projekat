using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class PoliseForma : Form
    {
        private readonly bool _samoPregled;

        public PoliseForma()
        {
            InitializeComponent();
            _samoPregled = SesijaKorisnik.ImaUlogu("LEKAR", "PRAVNIK", "PROCENITELJ");
            if (_samoPregled)
            {
                btnDodaj.Visible  = false;
                btnIzmeni.Visible = false;
                btnObrisi.Visible = false;
                UiHelper.PoravnajTraku(404, btnDodaj, btnIzmeni, btnObrisi, btnIstorija, btnOsvezi, lblBroj);
            }
            this.Load += (s, e) => this.BeginInvoke(new Action(ucitajPolise));
        }

        private void ucitajPolise()
        {
            try
            {
                var lista = DTOManager.vratiSvePolise(
                    cmbTip.SelectedItem?.ToString(),
                    cmbStatus.SelectedItem?.ToString());

                dgvPolise.SelectionChanged -= dgvPolise_SelectionChanged;
                dgvPolise.Rows.Clear();
                foreach (var p in lista)
                {
                    int r = dgvPolise.Rows.Add(
                        p.PolisaId, p.BrojPolise, p.TipOsiguranja,
                        p.UgovaracNaziv,
                        $"{p.OsnovnaPremija:N2} {p.Valuta}",
                        p.Status,
                        p.DatumPocetka.ToString("dd.MM.yyyy"),
                        p.DatumIsteka.ToString("dd.MM.yyyy"));

                    dgvPolise.Rows[r].Cells["pColStatus"].Style.ForeColor = UiHelper.StatusBoja(p.Status);
                    dgvPolise.Rows[r].Cells["pColStatus"].Style.Font =
                        new Font("Segoe UI", 8.5f, FontStyle.Bold);

                    if (p.Status == "AKTIVNA"
                        && p.DatumIsteka <= DateTime.Today.AddDays(30)
                        && p.DatumIsteka >= DateTime.Today)
                        dgvPolise.Rows[r].DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);

                    dgvPolise.Rows[r].Tag = p;
                }
                dgvPolise.SelectionChanged += dgvPolise_SelectionChanged;
                if (dgvPolise.Rows.Count > 0) dgvPolise.FirstDisplayedScrollingRowIndex = 0;
                lblBroj.Text = $"Ukupno: {lista.Count}";
                dgvPolise_SelectionChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška: " + ex.Message, "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private PolisaPregled? odabranaPolisa() =>
            dgvPolise.CurrentRow?.Tag as PolisaPregled;

        private void cmbTip_SelectedIndexChanged(object? sender, EventArgs e)    => ucitajPolise();
        private void cmbStatus_SelectedIndexChanged(object? sender, EventArgs e) => ucitajPolise();
        private void btnOsvezi_Click(object? sender, EventArgs e)                => ucitajPolise();

        private void dgvPolise_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnIzmeni_Click(sender, e);
        }

        private void btnDodaj_Click(object? sender, EventArgs e)
        {
            if (_samoPregled) return;
            var f = new DodajPolisaForma();
            if (f.ShowDialog() == DialogResult.OK) ucitajPolise();
        }

        private void btnIzmeni_Click(object? sender, EventArgs e)
        {
            if (_samoPregled) return;
            var p = odabranaPolisa();
            if (p == null) { MessageBox.Show("Izaberite polisu.", "Info"); return; }
            var f = new IzmeniPolisaForma(DTOManager.vratiPolisuDetaljno(p.PolisaId));
            if (f.ShowDialog() == DialogResult.OK) ucitajPolise();
        }

        private void btnObrisi_Click(object? sender, EventArgs e)
        {
            if (_samoPregled) return;
            var p = odabranaPolisa();
            if (p == null) { MessageBox.Show("Izaberite polisu.", "Info"); return; }
            if (MessageBox.Show($"Obrisati polisu {p.BrojPolise}?", "Potvrda",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiPolisu(p.PolisaId))) return;
                ucitajPolise();
            }
        }

        private void btnIstorija_Click(object? sender, EventArgs e)
        {
            var p = odabranaPolisa();
            if (p == null) { MessageBox.Show("Izaberite polisu.", "Info"); return; }

            var lista = DTOManager.vratiIstorijuPolise(p.PolisaId);
            using var dlg = new Form
            {
                Text = $"Istorija — {p.BrojPolise}", Size = new Size(640, 440),
                StartPosition = FormStartPosition.CenterParent, Font = new Font("Segoe UI", 9f), BackColor = Color.White
            };
            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgv);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Datum", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tip promene", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Opis", FillWeight = 55 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Korisnik", FillWeight = 15 });
            if (lista.Count == 0)
                dgv.Rows.Add("", "", "(nema zabeleženih izmena)", "");
            foreach (var ip in lista)
                dgv.Rows.Add(ip.DatumPromene.ToString("dd.MM.yyyy HH:mm"), ip.TipPromene, ip.Opis, ip.KorisnikIme ?? "/");
            dlg.Controls.Add(dgv);
            dlg.ShowDialog(this);
        }

        private void dgvPolise_SelectionChanged(object? sender, EventArgs e)
        {
            var p = odabranaPolisa();
            if (p == null) return;

            lblDetaljiNaziv.Text = p.BrojPolise;
            rtbDetalji.Clear();
            rtbDetalji.AppendText($"Tip:            {p.TipOsiguranja}\n");
            rtbDetalji.AppendText($"Status:         {p.Status}\n");
            rtbDetalji.AppendText($"Ugovarač:       {p.UgovaracNaziv}\n");
            rtbDetalji.AppendText($"Agent:          {p.AgentIme ?? "/"}\n\n");
            rtbDetalji.AppendText("── Finansije ─────────────────────────\n");
            rtbDetalji.AppendText($"Premija:        {p.OsnovnaPremija:N2} {p.Valuta}\n");
            rtbDetalji.AppendText($"Način plaćanja: {p.NacinPlacanja}\n\n");
            rtbDetalji.AppendText("── Trajanje ──────────────────────────\n");
            rtbDetalji.AppendText($"Važi od:        {p.DatumPocetka:dd.MM.yyyy}\n");
            rtbDetalji.AppendText($"Važi do:        {p.DatumIsteka:dd.MM.yyyy}\n");
            int dana = (int)(p.DatumIsteka - DateTime.Today).TotalDays;
            if (p.Status == "AKTIVNA")
                rtbDetalji.AppendText($"Preostalo:      {dana} dana\n");

            try
            {
                var detalji = DTOManager.vratiPolisuDetaljno(p.PolisaId);
                switch (detalji)
                {
                    case ZivotnoPregled z:
                        rtbDetalji.AppendText($"\n── Životno osiguranje ─────────────────\n");
                        rtbDetalji.AppendText($"Suma osiguranja: {z.SumaOsiguranja:N2} {p.Valuta}\n");
                        rtbDetalji.AppendText($"Tip isplate:     {z.TipIsplate}\n");
                        break;
                    case ZdravstvenoPregled zd:
                        rtbDetalji.AppendText($"\n── Zdravstveno osiguranje ─────────────\n");
                        rtbDetalji.AppendText($"Mreža ustanova:  {zd.MrezaUstanova}\n");
                        rtbDetalji.AppendText($"Pokrića:         {zd.Pokrica}\n");
                        rtbDetalji.AppendText($"Limiti: specijalisti {zd.LimitSpecijalista:N2} / stomatolog {zd.LimitStomatologa:N2} / bolničke int. {zd.LimitBolnickih:N2} / bolnički dan {zd.LimitBolnickiDan:N2}\n");
                        break;
                    case AutoPolisaPregled a:
                        rtbDetalji.AppendText($"\n── Auto osiguranje ────────────────────\n");
                        rtbDetalji.AppendText($"Vozilo:                {a.VoziloOpis}\n");
                        rtbDetalji.AppendText($"Bonus-malus klasa:     {a.BonusMalusKlasa}\n");
                        rtbDetalji.AppendText($"Teritorijalno važenje: {a.TeritorijanoVazenje}\n");
                        var vozaci = DTOManager.vratiVozaceAutoOsiguranja(p.PolisaId);
                        if (vozaci.Count > 0) rtbDetalji.AppendText($"Dodatni vozači: {vozaci.Count}\n");
                        break;
                    case ImovinskoPregled im:
                        rtbDetalji.AppendText($"\n── Imovinsko osiguranje ───────────────\n");
                        rtbDetalji.AppendText($"Vrste rizika: {im.VrsteRizika}\n");
                        rtbDetalji.AppendText($"Nekretnine: {im.NekretnineIds.Count}, Pokretna imovina: {im.PokretnaImovinaIds.Count}\n");
                        break;
                    case PutnoPregled pu:
                        rtbDetalji.AppendText($"\n── Putno osiguranje ───────────────────\n");
                        rtbDetalji.AppendText($"Destinacije:     {pu.Destinacije}\n");
                        rtbDetalji.AppendText($"Period:          {pu.DatumPolaska:dd.MM.yyyy} — {pu.DatumPovratka:dd.MM.yyyy}\n");
                        rtbDetalji.AppendText($"Osigurana lica:  {pu.OsiguranaLicaIds.Count}\n");
                        break;
                    case PoljoprivrednoPregled polj:
                        rtbDetalji.AppendText($"\n── Poljoprivredno osiguranje ──────────\n");
                        rtbDetalji.AppendText($"Usevi: {polj.UseviIds.Count}, Životinje: {polj.ZivotinjeIds.Count}\n");
                        break;
                    case OdgovornostPregled od:
                        rtbDetalji.AppendText($"\n── Osiguranje odgovornosti ────────────\n");
                        rtbDetalji.AppendText($"Vrsta:  {od.VrstaOdgovornosti}\n");
                        rtbDetalji.AppendText($"Limit:  {od.LimitOdgovornosti?.ToString("N2") ?? "/"}\n");
                        break;
                    case SpecijalizovanoPregled sp:
                        rtbDetalji.AppendText($"\n── Specijalizovano osiguranje ─────────\n");
                        rtbDetalji.AppendText($"Naziv:  {sp.NazivSpecijalizacije}\n");
                        rtbDetalji.AppendText($"Uslovi: {sp.OpisUslova}\n");
                        break;
                }

                var pb = (PolisaPregled)detalji;
                if (pb.DodatnaPokrića.Count > 0)
                {
                    rtbDetalji.AppendText($"\n── Dodatna pokrića ({pb.DodatnaPokrića.Count}) ──────────\n");
                    foreach (var dp in pb.DodatnaPokrića)
                        rtbDetalji.AppendText($"• {dp.Naziv} — +{dp.DodatnaPremija:N2} {p.Valuta}\n");
                }

                if (p.TipOsiguranja != "PUTNO")
                {
                    var uloge = DTOManager.vratiUlogeZaPolisu(p.PolisaId);
                    if (uloge.Count > 0)
                    {
                        rtbDetalji.AppendText($"\n── Osiguranici ({uloge.Count}) ──────────\n");
                        foreach (var u in uloge) rtbDetalji.AppendText($"• {u}\n");
                    }
                }

                if (p.TipOsiguranja == "ZIVOTNO")
                {
                    var korisnici = DTOManager.vratiKorisnikeIsplateZaPolisu(p.PolisaId);
                    if (korisnici.Count > 0)
                    {
                        rtbDetalji.AppendText($"\n── Korisnici isplate ({korisnici.Count}) ──────────\n");
                        foreach (var k in korisnici) rtbDetalji.AppendText($"• {k} — {k.ProcenatUdela:N2}%\n");
                    }
                }
            }
            catch { }
        }
    }
}
