using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class UpravljanjeNalozimaForma : Form
    {
        public UpravljanjeNalozimaForma()
        {
            InitializeComponent();
            this.Load += (s, e) => this.BeginInvoke(new Action(ucitajNaloge));
        }

        private void ucitajNaloge()
        {
            try
            {
                string? status = cmbStatus.SelectedItem?.ToString();
                var lista = status == "SVE" ? DTOManager.vratiSveNaloge()
                                             : DTOManager.vratiSveNaloge().FindAll(n => n.StatusNaloga == status);

                dgvNalozi.SelectionChanged -= dgvNalozi_SelectionChanged;
                dgvNalozi.Rows.Clear();
                foreach (var n in lista)
                {
                    int r = dgvNalozi.Rows.Add(
                        n.NalogId,
                        n.KorisnickoIme,
                        n.ImeOsoblja != null ? $"{n.ImeOsoblja} {n.PrezimeOsoblja}" : "/",
                        n.Uloga,
                        n.StatusNaloga,
                        n.NeuspesnihPrijava,
                        n.DatumRegistracije.ToString("dd.MM.yyyy"),
                        n.ZadnjaPrijava?.ToString("dd.MM.yyyy HH:mm") ?? "/");

                    dgvNalozi.Rows[r].Tag = n;
                    stilizujRedNaloga(dgvNalozi.Rows[r], n);
                }
                dgvNalozi.SelectionChanged += dgvNalozi_SelectionChanged;
                if (dgvNalozi.Rows.Count > 0) dgvNalozi.FirstDisplayedScrollingRowIndex = 0;
                lblBroj.Text = $"Ukupno: {lista.Count}";
                dgvNalozi_SelectionChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška: " + ex.Message, "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void stilizujRedNaloga(DataGridViewRow red, NalogPregled n)
        {
            red.Cells["nColStatus"].Style.ForeColor = UiHelper.StatusBoja(n.StatusNaloga);
            red.Cells["nColStatus"].Style.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            if (n.Uloga == "ADMIN")
                red.DefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);
        }

        private void dgvNalozi_Sorted(object? sender, EventArgs e)
        {
            int idx = -1;
            for (int i = 0; i < dgvNalozi.Rows.Count; i++)
                if (dgvNalozi.Rows[i].Tag is NalogPregled np && np.Uloga == "ADMIN") { idx = i; break; }
            if (idx <= 0) return;

            dgvNalozi.SelectionChanged -= dgvNalozi_SelectionChanged;
            var stariRed  = dgvNalozi.Rows[idx];
            var vrednosti = new object?[stariRed.Cells.Count];
            for (int c = 0; c < stariRed.Cells.Count; c++) vrednosti[c] = stariRed.Cells[c].Value;
            var n = (NalogPregled)stariRed.Tag!;

            dgvNalozi.Rows.RemoveAt(idx);
            dgvNalozi.Rows.Insert(0, vrednosti);
            var noviRed = dgvNalozi.Rows[0];
            noviRed.Tag = n;
            stilizujRedNaloga(noviRed, n);

            dgvNalozi.SelectionChanged += dgvNalozi_SelectionChanged;
            dgvNalozi_SelectionChanged(this, EventArgs.Empty);
        }

        private NalogPregled? odabraniNalog() => dgvNalozi.CurrentRow?.Tag as NalogPregled;

        private void cmbStatus_SelectedIndexChanged(object? sender, EventArgs e) => ucitajNaloge();
        private void btnOsvezi_Click(object? sender, EventArgs e)                => ucitajNaloge();

        private void btnOdobri_Click(object? sender, EventArgs e)
        {
            var n = odabraniNalog();
            if (n == null) { MessageBox.Show("Izaberite nalog.", "Info"); return; }
            if (n.StatusNaloga != "NA_CEKANJU")
            { MessageBox.Show("Samo nalozi na čekanju mogu biti odobreni.", "Info"); return; }
            if (string.IsNullOrEmpty(n.TipOsoblja))
            { MessageBox.Show("Nalogu nije dodeljen zaposleni sa definisanom ulogom.", "Greška"); return; }

            if (MessageBox.Show(
                    $"Odobriti zahtev za nalog \"{n.KorisnickoIme}\" ({n.ImeOsoblja} {n.PrezimeOsoblja}) sa ulogom \"{n.TipOsoblja}\"?",
                    "Potvrda odobrenja", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            if (!UiHelper.PokusajAkciju(() => DTOManager.odobriNalog(n.NalogId, n.TipOsoblja!))) return;
            ucitajNaloge();
        }

        private void btnOdbij_Click(object? sender, EventArgs e)
        {
            var n = odabraniNalog();
            if (n == null) { MessageBox.Show("Izaberite nalog.", "Info"); return; }
            if (MessageBox.Show($"Odbiti zahtev za nalog \"{n.KorisnickoIme}\"?", "Potvrda",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (!UiHelper.PokusajAkciju(() => DTOManager.odbijNalog(n.NalogId))) return;
            ucitajNaloge();
        }

        private void btnZakljucaj_Click(object? sender, EventArgs e)
        {
            var n = odabraniNalog();
            if (n == null) { MessageBox.Show("Izaberite nalog.", "Info"); return; }
            if (!UiHelper.PokusajAkciju(() => DTOManager.zakljucajNalog(n.NalogId))) return;
            ucitajNaloge();
        }

        private void btnOtkljucaj_Click(object? sender, EventArgs e)
        {
            var n = odabraniNalog();
            if (n == null) { MessageBox.Show("Izaberite nalog.", "Info"); return; }
            if (!UiHelper.PokusajAkciju(() => DTOManager.otkljucajNalog(n.NalogId))) return;
            ucitajNaloge();
        }

        private void btnResetuj_Click(object? sender, EventArgs e)
        {
            var n = odabraniNalog();
            if (n == null) { MessageBox.Show("Izaberite nalog.", "Info"); return; }
            using var f = new UnesiLozinkuForma();
            if (f.ShowDialog(this) != DialogResult.OK) return;
            if (!UiHelper.PokusajAkciju(() => DTOManager.resetujLozinku(n.NalogId, f.Lozinka))) return;
            MessageBox.Show("Privremena lozinka je postavljena. Korisnik mora da je promeni pri sledećoj prijavi.", "Готово");
        }

        private void btnObrisi_Click(object? sender, EventArgs e)
        {
            var n = odabraniNalog();
            if (n == null) { MessageBox.Show("Izaberite nalog.", "Info"); return; }
            if (n.Uloga == "ADMIN")
            { MessageBox.Show("Admin nalozi ne mogu biti obrisani iz ovog ekrana.", "Info"); return; }
            if (MessageBox.Show($"Trajno obrisati nalog \"{n.KorisnickoIme}\"? Ova akcija se ne može poništiti.", "Potvrda",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiNalog(n.NalogId))) return;
            ucitajNaloge();
        }

        private void dgvNalozi_SelectionChanged(object? sender, EventArgs e)
        {
            var n = odabraniNalog();
            bool mozeOdobritiOdbiti = n != null && n.Uloga != "ADMIN" && n.StatusNaloga == "NA_CEKANJU";
            btnOdobri.Enabled = mozeOdobritiOdbiti;
            btnOdbij.Enabled  = mozeOdobritiOdbiti;

            bool mozeZakljucatiOtkljucati = n != null && n.Uloga != "ADMIN";
            btnZakljucaj.Enabled = mozeZakljucatiOtkljucati;
            btnOtkljucaj.Enabled = mozeZakljucatiOtkljucati;

            if (n == null) { rtbDetalji.Clear(); return; }

            lblDetaljiNaziv.Text = n.KorisnickoIme;
            rtbDetalji.Clear();
            rtbDetalji.AppendText($"Zaposleni:        {(n.ImeOsoblja != null ? $"{n.ImeOsoblja} {n.PrezimeOsoblja}" : "/ (admin nalog)")}\n");
            rtbDetalji.AppendText($"Uloga:            {n.Uloga}\n");
            rtbDetalji.AppendText($"Status naloga:    {n.StatusNaloga}\n");
            rtbDetalji.AppendText($"Neuspešnih prijava: {n.NeuspesnihPrijava}\n");
            rtbDetalji.AppendText($"Datum registracije: {n.DatumRegistracije:dd.MM.yyyy HH:mm}\n");
            rtbDetalji.AppendText($"Datum odobrenja:  {(n.DatumOdobrenja != null ? n.DatumOdobrenja.Value.ToString("dd.MM.yyyy HH:mm") : "/")}\n");
            rtbDetalji.AppendText($"Zadnja prijava:   {(n.ZadnjaPrijava != null ? n.ZadnjaPrijava.Value.ToString("dd.MM.yyyy HH:mm") : "/")}\n");
            rtbDetalji.AppendText($"Prisilna promena lozinke: {(n.MoraPromenitiLozinku ? "DA" : "NE")}\n");
        }
    }

    public class UnesiLozinkuForma : Form
    {
        private TextBox txtLozinka = null!;
        private Button  btnOk = null!, btnCancel = null!;
        public string   Lozinka => txtLozinka.Text;

        public UnesiLozinkuForma()
        {
            this.Text            = "Privremena lozinka";
            this.Size            = new Size(380, 220);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(2);
            txtLozinka = UiHelper.DodajRed(tbl, 0, "Nova lozinka *:");

            var (ok, cancel) = UiHelper.DodajDugmadPanel(tbl, 1, "✔  Postavi", "✖  Odustani");
            btnOk = ok; btnCancel = cancel;
            btnOk.Click += (s, e) =>
            {
                if (txtLozinka.Text.Length < 8 || !System.Linq.Enumerable.Any(txtLozinka.Text, char.IsDigit))
                { MessageBox.Show("Lozinka mora imati bar 8 karaktera i bar jednu cifru.", "Validacija"); return; }
                DialogResult = DialogResult.OK;
                Close();
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
