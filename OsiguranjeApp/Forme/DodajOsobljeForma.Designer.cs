using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class DodajOsobljeForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox  txtIme = null!, txtPrezime = null!, txtJmbg = null!, txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private ComboBox cmbTip = null!, cmbStatus = null!, cmbTipPravnika = null!;
        private TextBox  txtBarBroj = null!, txtBrojLicence = null!, txtSpecijalizacija = null!;
        private TableLayoutPanel tbl = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        private void InitializeComponent()
        {
            this.Text            = "Dodaj zaposlenog";
            this.Size            = new Size(450, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            tbl = UiHelper.NapraviLayout(13);

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

            txtBrojLicence = UiHelper.DodajRed(tbl, 10, "Broj licence:");
            txtSpecijalizacija = UiHelper.DodajRed(tbl, 11, "Specijalizacija:");

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 12);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
            AzurirajVidljivostPoTipu();
        }
    }
}
