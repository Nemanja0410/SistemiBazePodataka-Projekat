using System;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class DodajAgentaForma : Form
    {
        public DodajAgentaForma()
        {
            InitializeComponent();
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIme.Text))
            { MessageBox.Show("Ime je obavezno.", "Validacija"); txtIme.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtPrezime.Text))
            { MessageBox.Show("Prezime je obavezno.", "Validacija"); txtPrezime.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtJmbg.Text) || txtJmbg.Text.Length != 13)
            { MessageBox.Show("JMBG mora imati 13 cifara.", "Validacija"); txtJmbg.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtLicenca.Text))
            { MessageBox.Show("Licenca je obavezna za agente.", "Validacija"); txtLicenca.Focus(); return; }

            decimal prov = 0;
            if (!string.IsNullOrWhiteSpace(txtProvizija.Text))
                decimal.TryParse(txtProvizija.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out prov);

            var dto = new AgentBasic
            {
                Ime = txtIme.Text.Trim(),
                Prezime = txtPrezime.Text.Trim(),
                Jmbg = txtJmbg.Text.Trim(),
                Adresa = txtAdresa.Text.Trim(),
                Telefon  = txtTelefon.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                TipAgenta = cmbTipAgenta.SelectedItem?.ToString(),
                Licenca = txtLicenca.Text.Trim(),
                RegionRada= txtRegion.Text.Trim(),
                ProvizijaProcenat = prov,
                Status= cmbStatus.SelectedItem?.ToString(),
                TipOsoblja = "AGENT"
            };

            if (!UiHelper.PokusajAkciju(() => DTOManager.dodajAgenta(dto))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
