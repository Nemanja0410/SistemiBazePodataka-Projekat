using System;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class DodajInstitucijaForma : Form
    {
        public DodajInstitucijaForma()
        {
            InitializeComponent();
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNaziv.Text))
            { MessageBox.Show("Naziv je obavezan.", "Validacija"); txtNaziv.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtPib.Text))
            { MessageBox.Show("PIB je obavezan.", "Validacija"); txtPib.Focus(); return; }

            var dto = new JavnaInstitucijaBasic
            {
                Naziv           = txtNaziv.Text.Trim(),
                Pib             = txtPib.Text.Trim(),
                MaticniBroj     = txtMb.Text.Trim(),
                Delatnost       = txtDelatnost.Text.Trim(),
                Adresa          = txtAdresa.Text.Trim(),
                Telefon         = txtTelefon.Text.Trim(),
                Email           = txtEmail.Text.Trim(),
                NivoInstitucije = cmbNivo.SelectedItem?.ToString(),
                Status          = cmbStatus.SelectedItem?.ToString(),
                TipKlijenta     = "JAVNA_INSTITUCIJA"
            };

            if (!UiHelper.PokusajAkciju(() => DTOManager.dodajJavnuInstituciju(dto))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
