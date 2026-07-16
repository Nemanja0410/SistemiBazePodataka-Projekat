using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class RegisterForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private ComboBox cmbZaposleni  = null!;
        private TextBox  txtKorisnickoIme = null!, txtLozinka = null!, txtPotvrda = null!;
        private Button   btnRegistruj = null!, btnOdustani = null!;

        private void InitializeComponent()
        {
            this.Text            = "Registracija naloga";
            this.Size            = new Size(450, 340);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(5);

            cmbZaposleni     = UiHelper.DodajComboRed(tbl, 0, "Zaposleni *:");
            txtKorisnickoIme = UiHelper.DodajRed(tbl, 1, "Korisničko ime *:");
            txtLozinka       = UiHelper.DodajRed(tbl, 2, "Lozinka *:");
            txtLozinka.UseSystemPasswordChar = true;
            txtPotvrda       = UiHelper.DodajRed(tbl, 3, "Potvrda lozinke *:");
            txtPotvrda.UseSystemPasswordChar = true;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 4, "✔  Registruj se", "✖  Odustani");
            btnRegistruj = btnOk;
            btnOdustani  = btnCancel;
            btnRegistruj.Click += BtnRegistruj_Click;
            btnOdustani.Click  += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
            this.AcceptButton = btnRegistruj;
            this.CancelButton = btnOdustani;
        }
    }
}
