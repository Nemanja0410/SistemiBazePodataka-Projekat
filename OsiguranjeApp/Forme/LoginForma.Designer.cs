using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class LoginForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox   txtKorisnickoIme = null!, txtLozinka = null!;
        private Button    btnPrijava = null!, btnIzlaz = null!;
        private LinkLabel lnkRegistracija = null!;

        private void InitializeComponent()
        {
            this.Text            = "Prijava";
            this.Size            = new Size(440, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var naslov = UiHelper.NapraviNaslov("🔒  Prijava u sistem");

            var tbl = UiHelper.NapraviLayout(3);
            txtKorisnickoIme = UiHelper.DodajRed(tbl, 0, "Korisničko ime:");
            txtLozinka       = UiHelper.DodajRed(tbl, 1, "Lozinka:");
            txtLozinka.UseSystemPasswordChar = true;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 2, "✔  Prijavi se", "✖  Izlaz");
            btnPrijava = btnOk;
            btnIzlaz   = btnCancel;
            btnPrijava.Click += BtnPrijava_Click;
            btnIzlaz.Click   += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            var pnlRegistracija = new Panel { Dock = DockStyle.Bottom, Height = 36 };
            lnkRegistracija = new LinkLabel
            {
                Text      = "Nemate nalog? Registrujte se",
                AutoSize  = true,
                Location  = new Point(16, 8)
            };
            lnkRegistracija.Click += LnkRegistracija_Click;
            pnlRegistracija.Controls.Add(lnkRegistracija);

            this.Controls.Add(tbl);
            this.Controls.Add(pnlRegistracija);
            this.Controls.Add(naslov);

            this.AcceptButton = btnPrijava;
            this.CancelButton = btnIzlaz;
        }
    }
}
