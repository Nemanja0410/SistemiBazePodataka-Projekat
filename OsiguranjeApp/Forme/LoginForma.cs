using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    public partial class LoginForma : Form
    {
        public LoginForma()
        {
            InitializeComponent();
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

            // Administrator je resetovao lozinku - korisnik mora da postavi novu pre nastavka.
            if (rezultat.Nalog!.MoraPromenitiLozinku)
            {
                if (!ZatraziNovuLozinku(rezultat.Nalog.NalogId, txtLozinka.Text))
                {
                    SesijaKorisnik.TrenutniNalog = null;
                    txtLozinka.Clear();
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        // Prisilna promena lozinke - vraca true tek kad korisnik uspesno postavi novu lozinku.
        private bool ZatraziNovuLozinku(int nalogId, string staraLozinka)
        {
            using var dlg = new Form
            {
                Text = "Postavite novu lozinku",
                Size = new Size(400, 220),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent,
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.White
            };

            var tbl = UiHelper.NapraviLayout(3);
            var lbl = new Label
            {
                Text = "Administrator je resetovao vašu lozinku.\nMorate postaviti novu pre nastavka.",
                Dock = DockStyle.Fill, AutoSize = false
            };
            tbl.Controls.Add(lbl, 0, 0);
            tbl.SetColumnSpan(lbl, 2);
            tbl.RowStyles[0].Height = 50;

            var txtNova = UiHelper.DodajRed(tbl, 1, "Nova lozinka:");
            txtNova.UseSystemPasswordChar = true;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 2, "✔  Postavi", "✖  Odustani");
            dlg.Controls.Add(tbl);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            bool uspesno = false;
            btnOk.Click += (s, e) =>
            {
                string nova = txtNova.Text;
                if (nova.Length < 8 || !nova.Any(char.IsDigit))
                {
                    MessageBox.Show("Lozinka mora imati najmanje 8 karaktera i bar jednu cifru.", "Validacija");
                    return;
                }
                var (ok, poruka) = DTOManager.promeniLozinku(nalogId, staraLozinka, nova);
                if (!ok) { MessageBox.Show(poruka, "Greška"); return; }
                uspesno = true;
                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            };
            btnCancel.Click += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

            dlg.ShowDialog(this);
            return uspesno;
        }

        private void LnkRegistracija_Click(object? sender, EventArgs e)
        {
            using var reg = new RegisterForma();
            reg.ShowDialog(this);
        }
    }
}
