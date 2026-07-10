using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class RegisterForma : Form
    {
        private ComboBox cmbZaposleni  = null!;
        private TextBox  txtKorisnickoIme = null!, txtLozinka = null!, txtPotvrda = null!;
        private Button   btnRegistruj = null!, btnOdustani = null!;

        public RegisterForma()
        {
            InitializeComponent();
            UcitajZaposlene();
        }

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

        private void UcitajZaposlene()
        {
            cmbZaposleni.Items.Clear();
            foreach (var o in DTOManager.vratiOsobljeZaRegistraciju())
                cmbZaposleni.Items.Add(new ComboItem(o.OsobljeId, $"{o.Ime} {o.Prezime} ({o.TipOsoblja})"));
            if (cmbZaposleni.Items.Count > 0)
                cmbZaposleni.SelectedIndex = 0;
        }

        private void BtnRegistruj_Click(object? sender, EventArgs e)
        {
            if (cmbZaposleni.SelectedItem == null)
            { MessageBox.Show("Nema dostupnih zaposlenih za registraciju (svi već imaju nalog).", "Validacija"); return; }
            if (string.IsNullOrWhiteSpace(txtKorisnickoIme.Text))
            { MessageBox.Show("Korisničko ime je obavezno.", "Validacija"); txtKorisnickoIme.Focus(); return; }
            if (txtLozinka.Text != txtPotvrda.Text)
            { MessageBox.Show("Lozinka i potvrda lozinke se ne poklapaju.", "Validacija"); txtPotvrda.Focus(); return; }

            var zahtev = new RegistracijaZahtev
            {
                OsobljeId     = ((ComboItem)cmbZaposleni.SelectedItem!).Id,
                KorisnickoIme = txtKorisnickoIme.Text.Trim(),
                Lozinka       = txtLozinka.Text
            };

            var (uspeh, poruka) = DTOManager.registrujNalog(zahtev);
            MessageBox.Show(poruka, uspeh ? "Registracija poslata" : "Greška",
                MessageBoxButtons.OK, uspeh ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (uspeh)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
