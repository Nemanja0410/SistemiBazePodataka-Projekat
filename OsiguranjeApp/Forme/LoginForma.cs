using System;
using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    public class LoginForma : Form
    {
        private TextBox   txtKorisnickoIme = null!, txtLozinka = null!;
        private Button    btnPrijava = null!, btnIzlaz = null!;
        private LinkLabel lnkRegistracija = null!;

        public LoginForma()
        {
            InitializeComponent();
        }

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

        private void BtnPrijava_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKorisnickoIme.Text) || string.IsNullOrWhiteSpace(txtLozinka.Text))
            {
                MessageBox.Show("Unesite korisničko ime i lozinku.", "Validacija");
                return;
            }

            var rezultat = DTOManager.prijaviSe(txtKorisnickoIme.Text.Trim(), txtLozinka.Text);
            if (!rezultat.Uspesno)
            {
                MessageBox.Show(rezultat.Poruka, "Neuspešna prijava", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLozinka.Clear();
                txtLozinka.Focus();
                return;
            }

            SesijaKorisnik.TrenutniNalog = rezultat.Nalog;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void LnkRegistracija_Click(object? sender, EventArgs e)
        {
            using var reg = new RegisterForma();
            reg.ShowDialog(this);
        }
    }
}
