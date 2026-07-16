using System;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class DodajPravnoLiceForma : Form
    {
        public DodajPravnoLiceForma()
        {
            InitializeComponent();
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNaziv.Text))
            { MessageBox.Show("Naziv je obavezan.", "Validacija"); txtNaziv.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtPib.Text))
            { MessageBox.Show("PIB je obavezan.", "Validacija"); txtPib.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtMb.Text))
            { MessageBox.Show("Matični broj je obavezan.", "Validacija"); txtMb.Focus(); return; }

            var dto = new PravnoLiceBasic
            {
                Naziv= txtNaziv.Text.Trim(),
                Pib= txtPib.Text.Trim(),
                MaticniBroj = txtMb.Text.Trim(),
                Delatnost   = txtDelatnost.Text.Trim(),
                Adresa= txtAdresa.Text.Trim(),
                Telefon= txtTelefon.Text.Trim(),
                Email= txtEmail.Text.Trim(),
                Status= cmbStatus.SelectedItem?.ToString(),
                TipKlijenta = "PRAVNO_LICE"
            };

            if (!UiHelper.PokusajAkciju(() => DTOManager.dodajPravnoLice(dto))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
