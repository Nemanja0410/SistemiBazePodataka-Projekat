using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class DodajFazuForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox       txtNaziv = null!, txtDokumentacija = null!, txtNapomena = null!;
        private ComboBox      cmbOdluka = null!, cmbOdgovornoLice = null!;
        private DateTimePicker dtpPocetka = null!, dtpZavrsetka = null!;
        private CheckBox      chkZavrsena = null!;
        private Button        btnSacuvaj = null!, btnOdustani = null!;

        private void InitializeComponent()
        {
            this.Text            = _postojeca == null
                ? $"Nova faza obrade (br. {_redniBroj})"
                : $"Izmeni fazu — {_postojeca.NazivFaze}";
            this.Size            = new Size(470, 440);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(9);

            txtNaziv   = UiHelper.DodajRed(tbl, 0, "Naziv faze *:");
            dtpPocetka = UiHelper.DodajDTPRed(tbl, 1, "Datum početka *:");

            chkZavrsena = new CheckBox
            {
                Text = "Faza je završena",
                Dock = DockStyle.Fill
            };
            chkZavrsena.CheckedChanged += (s, e) => dtpZavrsetka.Enabled = chkZavrsena.Checked;
            tbl.Controls.Add(chkZavrsena, 0, 2);
            tbl.SetColumnSpan(chkZavrsena, 2);

            dtpZavrsetka = UiHelper.DodajDTPRed(tbl, 3, "Datum završetka:");
            dtpZavrsetka.Enabled = false;

            cmbOdgovornoLice  = UiHelper.DodajComboRed(tbl, 4, "Odgovorno lice:");
            cmbOdluka         = UiHelper.DodajComboRed(tbl, 5, "Odluka:");
            cmbOdluka.Items.AddRange(new[] { "U_TOKU", "ODOBRENA", "ODBIJENA", "POTREBNA_DOPUNA" });
            cmbOdluka.SelectedIndex = 0;

            txtDokumentacija = UiHelper.DodajRed(tbl, 6, "Dokumentacija:");
            txtNapomena      = UiHelper.DodajRed(tbl, 7, "Napomena:");

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 8);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
        }
    }
}
