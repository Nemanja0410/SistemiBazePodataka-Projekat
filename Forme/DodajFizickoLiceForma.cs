using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class DodajFizickoLiceForma : Form
    {
        private TextBox       txtNaziv = null!, txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private TextBox       txtJmbg = null!, txtZanimanje = null!;
        private DateTimePicker dtpRodjenje = null!;
        private ComboBox      cmbStatus = null!;
        private Button        btnSacuvaj = null!, btnOdustani = null!;

        public DodajFizickoLiceForma()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Dodaj fizičko lice";
            this.Size            = new Size(450, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(9);

            txtNaziv     = UiHelper.DodajRed(tbl, 0, "Ime i prezime *:");
            txtJmbg      = UiHelper.DodajRed(tbl, 1, "JMBG *:");
            txtJmbg.MaxLength = 13;
            txtAdresa    = UiHelper.DodajRed(tbl, 2, "Adresa:");
            txtTelefon   = UiHelper.DodajRed(tbl, 3, "Telefon:");
            txtEmail     = UiHelper.DodajRed(tbl, 4, "Email:");
            txtZanimanje = UiHelper.DodajRed(tbl, 5, "Zanimanje:");
            dtpRodjenje  = UiHelper.DodajDTPRed(tbl, 6, "Datum rođenja:");
            cmbStatus    = UiHelper.DodajComboRed(tbl, 7, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "BLOKIRAN" });
            cmbStatus.SelectedIndex = 0;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 8);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
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

            DTOManager.dodajFizickoLice(dto);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
