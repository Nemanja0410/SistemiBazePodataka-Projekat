using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class DodajAgentaForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox  txtIme = null!, txtPrezime = null!, txtJmbg = null!, txtAdresa = null!;
        private TextBox  txtTelefon = null!, txtEmail = null!, txtLicenca = null!, txtRegion = null!, txtProvizija = null!;
        private ComboBox cmbTipAgenta = null!, cmbStatus = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        private void InitializeComponent()
        {
            this.Text  = "Dodaj agenta";
            this.Size = new Size(450, 540);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox  = false;
            this.StartPosition= FormStartPosition.CenterParent;
            this.Font  = new Font("Segoe UI", 9f);
            this.BackColor  = Color.White;

            var tbl = UiHelper.NapraviLayout(12);

            txtIme  = UiHelper.DodajRed(tbl, 0, "Ime *:");
            txtPrezime= UiHelper.DodajRed(tbl, 1, "Prezime *:");
            txtJmbg  = UiHelper.DodajRed(tbl, 2, "JMBG *:");
            txtJmbg.MaxLength = 13;
            txtAdresa = UiHelper.DodajRed(tbl, 3, "Adresa:");
            txtTelefon = UiHelper.DodajRed(tbl, 4, "Telefon:");
            txtEmail  = UiHelper.DodajRed(tbl, 5, "Email:");

            cmbTipAgenta = UiHelper.DodajComboRed(tbl, 6, "Tip agenta:");
            cmbTipAgenta.Items.AddRange(new[] { "INTERNI", "EKSTERNI" });
            cmbTipAgenta.SelectedIndex = 0;

            txtLicenca = UiHelper.DodajRed(tbl, 7, "Licenca *:");
            txtRegion = UiHelper.DodajRed(tbl, 8, "Region rada:");
            txtProvizija = UiHelper.DodajRed(tbl, 9, "Provizija %:");

            cmbStatus= UiHelper.DodajComboRed(tbl, 10, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "SUSPENDOVAN" });
            cmbStatus.SelectedIndex = 0;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 11);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
        }
    }
}
