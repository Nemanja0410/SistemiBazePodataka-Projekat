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
        private ComboBox cmbStatus = null!, cmbTipPravnika = null!;
        private TextBox  txtBarBroj = null!, txtBrojLicence = null!;
        private TableLayoutPanel tbl = null!;
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
            this.Size            = new Size(450, 430);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            bool jePravnik     = _osoblje.TipOsoblja == "PRAVNIK";
            bool jeProcenitelj = _osoblje.TipOsoblja == "PROCENITELJ";

            tbl = UiHelper.NapraviLayout(9);

            txtIme     = UiHelper.DodajRed(tbl, 0, "Ime *:");
            txtPrezime = UiHelper.DodajRed(tbl, 1, "Prezime *:");
            txtAdresa  = UiHelper.DodajRed(tbl, 2, "Adresa:");
            txtTelefon = UiHelper.DodajRed(tbl, 3, "Telefon:");
            txtEmail   = UiHelper.DodajRed(tbl, 4, "Email:");
            cmbStatus  = UiHelper.DodajComboRed(tbl, 5, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "SUSPENDOVAN" });

            cmbTipPravnika = UiHelper.DodajComboRed(tbl, 6, "Tip pravnika:");
            cmbTipPravnika.Items.AddRange(new[] { "INTERNI", "EKSTERNI" });
            txtBarBroj = UiHelper.DodajRed(tbl, 7, "Broj advokata:");
            cmbTipPravnika.Visible = jePravnik;
            txtBarBroj.Visible     = jePravnik;
            tbl.GetControlFromPosition(0, 6)!.Visible = jePravnik;
            tbl.GetControlFromPosition(0, 7)!.Visible = jePravnik;
            tbl.RowStyles[6].Height = jePravnik ? 36 : 0;
            tbl.RowStyles[7].Height = jePravnik ? 36 : 0;

            txtBrojLicence = UiHelper.DodajRed(tbl, 6, "Broj licence:");
            txtBrojLicence.Visible = jeProcenitelj;
            tbl.GetControlFromPosition(0, 6)!.Visible = jeProcenitelj || jePravnik;
            if (jeProcenitelj) tbl.RowStyles[6].Height = 36;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 8);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);

            int visina = 300;
            if (jePravnik) visina += 72;
            if (jeProcenitelj) visina += 36;
            this.Height = visina;
        }

        private void PopuniFormu()
        {
            txtIme.Text            = _osoblje.Ime ?? "";
            txtPrezime.Text        = _osoblje.Prezime ?? "";
            txtAdresa.Text         = _osoblje.Adresa ?? "";
            txtTelefon.Text        = _osoblje.Telefon ?? "";
            txtEmail.Text          = _osoblje.Email ?? "";
            cmbStatus.SelectedItem = _osoblje.Status ?? "AKTIVAN";

            if (_osoblje is OsobljeBasic ob)
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

            if (!UiHelper.PokusajAkciju(() => DTOManager.azurirajOsoblje(dto))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
