using System;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class RegisterForma : Form
    {
        public RegisterForma()
        {
            InitializeComponent();
            UcitajZaposlene();
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
