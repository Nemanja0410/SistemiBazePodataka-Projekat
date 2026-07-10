using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class IzmeniOsobljeForma : Form
    {
        private readonly OsobljePregled _osoblje;
        private TextBox  txtIme = null!, txtPrezime = null!, txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private ComboBox cmbStatus = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        public IzmeniOsobljeForma(OsobljePregled o)
        {
            _osoblje = o;
            InitializeComponent();
            PopuniFormu();
        }

        private void InitializeComponent()
        {
            this.Text            = $"Izmeni — {_osoblje.Ime} {_osoblje.Prezime}";
            this.Size            = new Size(450, 360);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(7);

            txtIme     = UiHelper.DodajRed(tbl, 0, "Ime *:");
            txtPrezime = UiHelper.DodajRed(tbl, 1, "Prezime *:");
            txtAdresa  = UiHelper.DodajRed(tbl, 2, "Adresa:");
            txtTelefon = UiHelper.DodajRed(tbl, 3, "Telefon:");
            txtEmail   = UiHelper.DodajRed(tbl, 4, "Email:");
            cmbStatus  = UiHelper.DodajComboRed(tbl, 5, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "SUSPENDOVAN" });

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 6);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
        }

        private void PopuniFormu()
        {
            txtIme.Text            = _osoblje.Ime ?? "";
            txtPrezime.Text        = _osoblje.Prezime ?? "";
            txtAdresa.Text         = _osoblje.Adresa ?? "";
            txtTelefon.Text        = _osoblje.Telefon ?? "";
            txtEmail.Text          = _osoblje.Email ?? "";
            cmbStatus.SelectedItem = _osoblje.Status ?? "AKTIVAN";
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIme.Text))
            { MessageBox.Show("Ime je obavezno.", "Validacija"); txtIme.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtPrezime.Text))
            { MessageBox.Show("Prezime je obavezno.", "Validacija"); txtPrezime.Focus(); return; }

            var dto = new OsobljeBasic
            {
                OsobljeId  = _osoblje.OsobljeId,
                TipOsoblja = _osoblje.TipOsoblja,
                Ime        = txtIme.Text.Trim(),
                Prezime    = txtPrezime.Text.Trim(),
                Adresa     = txtAdresa.Text.Trim(),
                Telefon    = txtTelefon.Text.Trim(),
                Email      = txtEmail.Text.Trim(),
                Status     = cmbStatus.SelectedItem?.ToString()
            };

            if (!UiHelper.PokusajAkciju(() => DTOManager.azurirajOsoblje(dto))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
