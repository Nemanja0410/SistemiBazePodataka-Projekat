using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class DodajInstitucijaForma : Form
    {
        private TextBox  txtNaziv = null!, txtPib = null!, txtMb = null!, txtDelatnost = null!;
        private TextBox  txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private ComboBox cmbStatus = null!, cmbNivo = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        public DodajInstitucijaForma()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Dodaj javnu instituciju";
            this.Size            = new Size(460, 410);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(10);

            txtNaziv     = UiHelper.DodajRed(tbl, 0, "Naziv *:");
            txtPib       = UiHelper.DodajRed(tbl, 1, "PIB *:");
            txtPib.MaxLength = 9;
            txtMb        = UiHelper.DodajRed(tbl, 2, "Matični broj *:");
            txtMb.MaxLength = 8;
            txtDelatnost = UiHelper.DodajRed(tbl, 3, "Delatnost:");
            txtAdresa    = UiHelper.DodajRed(tbl, 4, "Adresa:");
            txtTelefon   = UiHelper.DodajRed(tbl, 5, "Telefon:");
            txtEmail     = UiHelper.DodajRed(tbl, 6, "Email:");
            cmbNivo      = UiHelper.DodajComboRed(tbl, 7, "Nivo institucije *:");
            cmbNivo.Items.AddRange(new[] { "REPUBLICKA", "POKRAJINSKA", "GRADSKA", "OPSTINSKA" });
            cmbNivo.SelectedIndex = 2;
            cmbStatus    = UiHelper.DodajComboRed(tbl, 8, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "BLOKIRAN" });
            cmbStatus.SelectedIndex = 0;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 9);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
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

            DTOManager.dodajJavnuInstituciju(dto);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
