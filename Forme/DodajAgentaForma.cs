using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class DodajAgentaForma : Form
    {
        private TextBox  txtIme = null!, txtPrezime = null!, txtJmbg = null!, txtAdresa = null!;
        private TextBox  txtTelefon = null!, txtEmail = null!, txtLicenca = null!, txtRegion = null!, txtProvizija = null!;
        private ComboBox cmbTipAgenta = null!, cmbStatus = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        public DodajAgentaForma()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Dodaj agenta";
            this.Size            = new Size(450, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(12);

            txtIme       = UiHelper.DodajRed(tbl, 0, "Ime *:");
            txtPrezime   = UiHelper.DodajRed(tbl, 1, "Prezime *:");
            txtJmbg      = UiHelper.DodajRed(tbl, 2, "JMBG *:");
            txtJmbg.MaxLength = 13;
            txtAdresa    = UiHelper.DodajRed(tbl, 3, "Adresa:");
            txtTelefon   = UiHelper.DodajRed(tbl, 4, "Telefon:");
            txtEmail     = UiHelper.DodajRed(tbl, 5, "Email:");

            cmbTipAgenta = UiHelper.DodajComboRed(tbl, 6, "Tip agenta:");
            cmbTipAgenta.Items.AddRange(new[] { "INTERNI", "EKSTERNI" });
            cmbTipAgenta.SelectedIndex = 0;

            txtLicenca   = UiHelper.DodajRed(tbl, 7, "Licenca *:");
            txtRegion    = UiHelper.DodajRed(tbl, 8, "Region rada:");
            txtProvizija = UiHelper.DodajRed(tbl, 9, "Provizija %:");

            cmbStatus    = UiHelper.DodajComboRed(tbl, 10, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "SUSPENDOVAN" });
            cmbStatus.SelectedIndex = 0;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 11);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
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
                Ime               = txtIme.Text.Trim(),
                Prezime           = txtPrezime.Text.Trim(),
                Jmbg              = txtJmbg.Text.Trim(),
                Adresa            = txtAdresa.Text.Trim(),
                Telefon           = txtTelefon.Text.Trim(),
                Email             = txtEmail.Text.Trim(),
                TipAgenta         = cmbTipAgenta.SelectedItem?.ToString(),
                Licenca           = txtLicenca.Text.Trim(),
                RegionRada        = txtRegion.Text.Trim(),
                ProvizijaProcenat = prov,
                Status            = cmbStatus.SelectedItem?.ToString(),
                TipOsoblja        = "AGENT"
            };

            DTOManager.dodajAgenta(dto);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
