using System;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class DodajFizickoLiceForma : Form
    {
        public DodajFizickoLiceForma()
        {
            InitializeComponent();
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNaziv.Text))
            { MessageBox.Show("Ime i prezime su obavezni.", "Validacija"); txtNaziv.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtJmbg.Text) || txtJmbg.Text.Length != 13)
            { MessageBox.Show("JMBG mora imati tačno 13 cifara.", "Validacija"); txtJmbg.Focus(); return; }

            var dto = new FizickoLiceBasic
            {
                Naziv         = txtNaziv.Text.Trim(),
                Jmbg          = txtJmbg.Text.Trim(),
                Adresa        = txtAdresa.Text.Trim(),
                Telefon       = txtTelefon.Text.Trim(),
                Email         = txtEmail.Text.Trim(),
                Zanimanje     = txtZanimanje.Text.Trim(),
                DatumRodjenja = dtpRodjenje.Value.Date,
                Status        = cmbStatus.SelectedItem?.ToString(),
                TipKlijenta   = "FIZICKO_LICE"
            };

            if (!UiHelper.PokusajAkciju(() => DTOManager.dodajFizickoLice(dto))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
