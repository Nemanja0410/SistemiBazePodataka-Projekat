using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class KlijentiForma : Form
    {
        private readonly bool _samoPregled;

        public KlijentiForma()
        {
            InitializeComponent();
            _samoPregled = SesijaKorisnik.ImaUlogu("LEKAR", "PRAVNIK", "PROCENITELJ");
            if (_samoPregled)
            {
                btnDodajFizicko.Visible     = false;
                btnDodajPravno.Visible      = false;
                btnDodajInstituciju.Visible = false;
                btnIzmeni.Visible           = false;
                btnObrisi.Visible           = false;
                UiHelper.PoravnajTraku(532, btnDodajFizicko, btnDodajPravno, btnDodajInstituciju,
                    btnIzmeni, btnObrisi, btnOsvezi, lblBroj);
            }
            this.Load += (s, e) => this.BeginInvoke(new Action(ucitajKlijente));
        }

        private void ucitajKlijente()
        {
            try
            {
                var lista = DTOManager.pretraziKlijente(
                    txtPretraga.Text.Trim(),
                    cmbTip.SelectedItem?.ToString());

                dgvKlijenti.SelectionChanged -= dgvKlijenti_SelectionChanged;
                dgvKlijenti.Rows.Clear();
                foreach (var k in lista)
                {
                    int r = dgvKlijenti.Rows.Add(
                        k.KlijentId,
                        k.Naziv,
                        k.TipKlijenta?.Replace("_", " "),
                        k.Telefon,
                        k.Email,
                        k.Status,
                        k.DatumRegistracije.ToString("dd.MM.yyyy"));

                    var boja = UiHelper.StatusBoja(k.Status);
                    dgvKlijenti.Rows[r].Cells["colStatus"].Style.ForeColor = boja;
                    dgvKlijenti.Rows[r].Cells["colStatus"].Style.Font =
                        new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    dgvKlijenti.Rows[r].Tag = k;
                }
                dgvKlijenti.SelectionChanged += dgvKlijenti_SelectionChanged;
                if (dgvKlijenti.Rows.Count > 0) dgvKlijenti.FirstDisplayedScrollingRowIndex = 0;
                lblBroj.Text = $"Ukupno: {lista.Count}";
                dgvKlijenti_SelectionChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška: " + ex.Message, "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private KlijentPregled? odabraniKlijent() =>
            dgvKlijenti.CurrentRow?.Tag as KlijentPregled;

        private void txtPretraga_TextChanged(object sender, EventArgs e) => ucitajKlijente();
        private void cmbTip_SelectedIndexChanged(object sender, EventArgs e) => ucitajKlijente();
        private void btnOsvezi_Click(object sender, EventArgs e) => ucitajKlijente();

        private void dgvKlijenti_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnIzmeni_Click(sender, e);
        }

        private void btnDodajFizicko_Click(object sender, EventArgs e)
        {
            if (_samoPregled) return;
            var f = new DodajFizickoLiceForma();
            if (f.ShowDialog() == DialogResult.OK) ucitajKlijente();
        }

        private void btnDodajPravno_Click(object sender, EventArgs e)
        {
            if (_samoPregled) return;
            var f = new DodajPravnoLiceForma();
            if (f.ShowDialog() == DialogResult.OK) ucitajKlijente();
        }

        private void btnDodajInstituciju_Click(object sender, EventArgs e)
        {
            if (_samoPregled) return;
            var f = new DodajInstitucijaForma();
            if (f.ShowDialog() == DialogResult.OK) ucitajKlijente();
        }

        private void btnIzmeni_Click(object sender, EventArgs e)
        {
            if (_samoPregled) return;
            var k = odabraniKlijent();
            if (k == null) { MessageBox.Show("Izaberite klijenta.", "Info"); return; }
            var f = new IzmeniKlijentaForma(k);
            if (f.ShowDialog() == DialogResult.OK) ucitajKlijente();
        }

        private void btnObrisi_Click(object sender, EventArgs e)
        {
            if (_samoPregled) return;
            var k = odabraniKlijent();
            if (k == null) { MessageBox.Show("Izaberite klijenta.", "Info"); return; }
            if (MessageBox.Show(
                    $"Obrisati klijenta \"{k.Naziv}\"?\n\nBiće obrisane i sve polise i štete!",
                    "Potvrda brisanja", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                == DialogResult.Yes)
            {
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiKlijenta(k.KlijentId))) return;
                ucitajKlijente();
                rtbDetalji.Clear();
                lblDetaljiNaziv.Text = "Detalji klijenta";
            }
        }

        private void dgvKlijenti_SelectionChanged(object? sender, EventArgs e)
        {
            var k = odabraniKlijent();
            if (k == null) return;

            lblDetaljiNaziv.Text = k.Naziv;
            rtbDetalji.Clear();
            rtbDetalji.AppendText($"ID:             {k.KlijentId}\n");
            rtbDetalji.AppendText($"Tip:            {k.TipKlijenta?.Replace("_", " ")}\n");
            rtbDetalji.AppendText($"Adresa:         {k.Adresa}\n");
            rtbDetalji.AppendText($"Telefon:        {k.Telefon}\n");
            rtbDetalji.AppendText($"Email:          {k.Email}\n");
            rtbDetalji.AppendText($"Status:         {k.Status}\n");
            rtbDetalji.AppendText($"Registrovan:    {k.DatumRegistracije:dd.MM.yyyy}\n\n");

            if (k is FizickoLicePregled fl)
            {
                rtbDetalji.AppendText("── Lični podaci ──────────────────────\n");
                rtbDetalji.AppendText($"JMBG:           {fl.Jmbg}\n");
                rtbDetalji.AppendText($"Datum rođenja:  {fl.DatumRodjenja:dd.MM.yyyy}\n");
                rtbDetalji.AppendText($"Zanimanje:      {fl.Zanimanje}\n");
            }
            else if (k is PravnoLicePregled pl)
            {
                rtbDetalji.AppendText("── Poslovni podaci ───────────────────\n");
                rtbDetalji.AppendText($"PIB:            {pl.Pib}\n");
                rtbDetalji.AppendText($"Matični broj:   {pl.MaticniBroj}\n");
                rtbDetalji.AppendText($"Delatnost:      {pl.Delatnost}\n");
            }
            else if (k is JavnaInstitucijaPregled ji)
            {
                rtbDetalji.AppendText("── Institucija ───────────────────────\n");
                rtbDetalji.AppendText($"PIB:            {ji.Pib}\n");
                rtbDetalji.AppendText($"Matični broj:   {ji.MaticniBroj}\n");
                rtbDetalji.AppendText($"Delatnost:      {ji.Delatnost}\n");
                rtbDetalji.AppendText($"Nivo:           {ji.NivoInstitucije}\n");
            }
        }
    }
}
