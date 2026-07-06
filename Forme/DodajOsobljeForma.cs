using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class DodajOsobljeForma : Form
    {
        private TextBox  txtIme = null!, txtPrezime = null!, txtJmbg = null!, txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private ComboBox cmbTip = null!, cmbStatus = null!, cmbTipPravnika = null!;
        private TextBox  txtBarBroj = null!;
        private TableLayoutPanel tbl = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        public DodajOsobljeForma()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Dodaj zaposlenog";
            this.Size            = new Size(450, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            tbl = UiHelper.NapraviLayout(11);

            cmbTip     = UiHelper.DodajComboRed(tbl, 0, "Tip *:");
            cmbTip.Items.AddRange(new[] { "PROCENITELJ", "LEKAR", "PRAVNIK", "OSTALO" });
            cmbTip.SelectedIndex = 0;
            cmbTip.SelectedIndexChanged += CmbTip_SelectedIndexChanged;

            txtIme     = UiHelper.DodajRed(tbl, 1, "Ime *:");
            txtPrezime = UiHelper.DodajRed(tbl, 2, "Prezime *:");
            txtJmbg    = UiHelper.DodajRed(tbl, 3, "JMBG *:");
            txtJmbg.MaxLength = 13;
            txtAdresa  = UiHelper.DodajRed(tbl, 4, "Adresa:");
            txtTelefon = UiHelper.DodajRed(tbl, 5, "Telefon:");
            txtEmail   = UiHelper.DodajRed(tbl, 6, "Email:");
            cmbStatus  = UiHelper.DodajComboRed(tbl, 7, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "SUSPENDOVAN" });
            cmbStatus.SelectedIndex = 0;

            cmbTipPravnika = UiHelper.DodajComboRed(tbl, 8, "Tip pravnika *:");
            cmbTipPravnika.Items.AddRange(new[] { "INTERNI", "EKSTERNI" });
            cmbTipPravnika.SelectedIndex = 0;
            txtBarBroj = UiHelper.DodajRed(tbl, 9, "Broj advokata:");

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 10);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
            AzurirajVidljivostPravnika();
        }

        private void CmbTip_SelectedIndexChanged(object? sender, EventArgs e) => AzurirajVidljivostPravnika();

        private void AzurirajVidljivostPravnika()
        {
            bool jePravnik = cmbTip.SelectedItem?.ToString() == "PRAVNIK";
            cmbTipPravnika.Visible = jePravnik;
            txtBarBroj.Visible     = jePravnik;
            tbl.GetControlFromPosition(0, 8)!.Visible = jePravnik;
            tbl.GetControlFromPosition(0, 9)!.Visible = jePravnik;
            tbl.RowStyles[8].Height = jePravnik ? 36 : 0;
            tbl.RowStyles[9].Height = jePravnik ? 36 : 0;
            this.Height = jePravnik ? 500 : 430;
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIme.Text))
            { MessageBox.Show("Ime je obavezno.", "Validacija"); txtIme.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtPrezime.Text))
            { MessageBox.Show("Prezime je obavezno.", "Validacija"); txtPrezime.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtJmbg.Text) || txtJmbg.Text.Length != 13)
            { MessageBox.Show("JMBG mora imati 13 cifara.", "Validacija"); txtJmbg.Focus(); return; }

            bool jePravnik = cmbTip.SelectedItem?.ToString() == "PRAVNIK";
            if (jePravnik && cmbTipPravnika.SelectedItem == null)
            { MessageBox.Show("Tip pravnika je obavezan.", "Validacija"); cmbTipPravnika.Focus(); return; }

            var dto = new OsobljeBasic
            {
                TipOsoblja  = cmbTip.SelectedItem?.ToString(),
                Ime         = txtIme.Text.Trim(),
                Prezime     = txtPrezime.Text.Trim(),
                Jmbg        = txtJmbg.Text.Trim(),
                Adresa      = txtAdresa.Text.Trim(),
                Telefon     = txtTelefon.Text.Trim(),
                Email       = txtEmail.Text.Trim(),
                Status      = cmbStatus.SelectedItem?.ToString(),
                TipPravnika = jePravnik ? cmbTipPravnika.SelectedItem?.ToString() : null,
                BarBroj     = jePravnik ? txtBarBroj.Text.Trim() : null
            };

            if (!UiHelper.PokusajAkciju(() => DTOManager.dodajOsoblje(dto))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
