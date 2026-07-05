using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class DodajPravnoLiceForma : Form
    {
        private TextBox  txtNaziv = null!, txtPib = null!, txtMb = null!, txtDelatnost = null!;
        private TextBox  txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private ComboBox cmbStatus = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        public DodajPravnoLiceForma()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Dodaj pravno lice";
            this.Size            = new Size(450, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(9);

            txtNaziv     = UiHelper.DodajRed(tbl, 0, "Naziv *:");
            txtPib       = UiHelper.DodajRed(tbl, 1, "PIB *:");
            txtPib.MaxLength = 9;
            txtMb        = UiHelper.DodajRed(tbl, 2, "Matični broj *:");
            txtMb.MaxLength = 8;
            txtDelatnost = UiHelper.DodajRed(tbl, 3, "Delatnost:");
            txtAdresa    = UiHelper.DodajRed(tbl, 4, "Adresa:");
            txtTelefon   = UiHelper.DodajRed(tbl, 5, "Telefon:");
            txtEmail     = UiHelper.DodajRed(tbl, 6, "Email:");
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
            { MessageBox.Show("Naziv je obavezan.", "Validacija"); txtNaziv.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtPib.Text))
            { MessageBox.Show("PIB je obavezan.", "Validacija"); txtPib.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtMb.Text))
            { MessageBox.Show("Matični broj je obavezan.", "Validacija"); txtMb.Focus(); return; }

            var dto = new PravnoLiceBasic
            {
                Naziv       = txtNaziv.Text.Trim(),
                Pib         = txtPib.Text.Trim(),
                MaticniBroj = txtMb.Text.Trim(),
                Delatnost   = txtDelatnost.Text.Trim(),
                Adresa      = txtAdresa.Text.Trim(),
                Telefon     = txtTelefon.Text.Trim(),
                Email       = txtEmail.Text.Trim(),
                Status      = cmbStatus.SelectedItem?.ToString(),
                TipKlijenta = "PRAVNO_LICE"
            };

            DTOManager.dodajPravnoLice(dto);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
