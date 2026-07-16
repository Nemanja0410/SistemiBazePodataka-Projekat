using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class DodajFizickoLiceForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox       txtNaziv = null!, txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private TextBox       txtJmbg = null!, txtZanimanje = null!;
        private DateTimePicker dtpRodjenje = null!;
        private ComboBox      cmbStatus = null!;
        private Button        btnSacuvaj = null!, btnOdustani = null!;

        private void InitializeComponent()
        {
            this.Text            = "Dodaj fizičko lice";
            this.Size            = new Size(450, 430);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(9);

            txtNaziv     = UiHelper.DodajRed(tbl, 0, "Ime i prezime *:");
            txtJmbg      = UiHelper.DodajRed(tbl, 1, "JMBG *:");
            txtJmbg.MaxLength = 13;
            txtAdresa    = UiHelper.DodajRed(tbl, 2, "Adresa:");
            txtTelefon   = UiHelper.DodajRed(tbl, 3, "Telefon:");
            txtEmail     = UiHelper.DodajRed(tbl, 4, "Email:");
            txtZanimanje = UiHelper.DodajRed(tbl, 5, "Zanimanje:");
            dtpRodjenje  = UiHelper.DodajDTPRed(tbl, 6, "Datum rođenja:");
            cmbStatus    = UiHelper.DodajComboRed(tbl, 7, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "BLOKIRAN" });
            cmbStatus.SelectedIndex = 0;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 8);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
        }
    }
}
