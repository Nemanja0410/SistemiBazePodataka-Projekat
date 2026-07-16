using System;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class IzmeniOsobljeForma : Form
    {
        private readonly OsobljePregled _osoblje;

        public IzmeniOsobljeForma(OsobljePregled o)
        {
            _osoblje = o;
            InitializeComponent();
            PopuniFormu();
        }

        private void PopuniFormu()
        {
            txtIme.Text            = _osoblje.Ime ?? "";
            txtPrezime.Text        = _osoblje.Prezime ?? "";
            txtAdresa.Text         = _osoblje.Adresa ?? "";
            txtTelefon.Text        = _osoblje.Telefon ?? "";
            txtEmail.Text          = _osoblje.Email ?? "";
            cmbStatus.SelectedItem = _osoblje.Status ?? "AKTIVAN";

            if (_osoblje is LekarBasic lb)
            {
                txtBrojLicence.Text     = lb.LicencaBroj ?? "";
                txtSpecijalizacija.Text = lb.Specijalizacija ?? "";
            }
            else if (_osoblje is OsobljeBasic ob)
            {
                cmbTipPravnika.SelectedItem = ob.TipPravnika;
                txtBarBroj.Text             = ob.BarBroj ?? "";
                txtBrojLicence.Text         = ob.BrojLicence ?? "";
            }
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIme.Text))
            { MessageBox.Show("Ime je obavezno.", "Validacija"); txtIme.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtPrezime.Text))
            { MessageBox.Show("Prezime je obavezno.", "Validacija"); txtPrezime.Focus(); return; }

            bool uspeh;
            if (_osoblje.TipOsoblja == "LEKAR")
            {
                var dtoLekar = new LekarBasic
                {
                    OsobljeId = _osoblje.OsobljeId,
                    Ime = txtIme.Text.Trim(), Prezime = txtPrezime.Text.Trim(),
                    Adresa = txtAdresa.Text.Trim(), Telefon = txtTelefon.Text.Trim(),
                    Email = txtEmail.Text.Trim(), Status = cmbStatus.SelectedItem?.ToString(),
                    LicencaBroj = txtBrojLicence.Text.Trim(),
                    Specijalizacija = txtSpecijalizacija.Text.Trim()
                };
                uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajLekara(dtoLekar));
            }
            else
            {
                var dto = new OsobljeBasic
                {
                    OsobljeId   = _osoblje.OsobljeId,
                    TipOsoblja  = _osoblje.TipOsoblja,
                    Ime         = txtIme.Text.Trim(),
                    Prezime     = txtPrezime.Text.Trim(),
                    Adresa      = txtAdresa.Text.Trim(),
                    Telefon     = txtTelefon.Text.Trim(),
                    Email       = txtEmail.Text.Trim(),
                    Status      = cmbStatus.SelectedItem?.ToString(),
                    TipPravnika = cmbTipPravnika.Visible ? cmbTipPravnika.SelectedItem?.ToString() : null,
                    BarBroj     = cmbTipPravnika.Visible ? txtBarBroj.Text.Trim() : null,
                    BrojLicence = txtBrojLicence.Visible ? txtBrojLicence.Text.Trim() : null
                };
                uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajOsoblje(dto));
            }

            if (!uspeh) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
