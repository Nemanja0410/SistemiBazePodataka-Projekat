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
                UiHelper.PoravnajTraku(404, btnDodaj, btnIzmeni, btnObrisi, btnOsvezi, lblBroj);
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
            var f = new IzmeniPolisaForma(DTOManager.vratiPolisu(p.PolisaId));
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
                var pb = DTOManager.vratiPolisu(p.PolisaId);
                if (pb.DodatnaPokrića.Count > 0)
                {
                    rtbDetalji.AppendText($"\n── Dodatna pokrića ({pb.DodatnaPokrića.Count}) ──────────\n");
                    foreach (var dp in pb.DodatnaPokrića)
                        rtbDetalji.AppendText($"• {dp.Naziv} — +{dp.DodatnaPremija:N2} RSD\n");
                }
            }
            catch { }
        }
    }
}
