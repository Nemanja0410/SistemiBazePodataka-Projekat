using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class DodajPravnoLiceForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox  txtNaziv = null!, txtPib = null!, txtMb = null!, txtDelatnost = null!;
        private TextBox  txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private ComboBox cmbStatus = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        private void InitializeComponent()
        {
            this.Text= "Dodaj pravno lice";
            this.Size= new Size(450, 430);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font= new Font("Segoe UI", 9f);
            this.BackColor= Color.White;

            var tbl = UiHelper.NapraviLayout(9);

            txtNaziv= UiHelper.DodajRed(tbl, 0, "Naziv *:");
            txtPib= UiHelper.DodajRed(tbl, 1, "PIB *:");
            txtPib.MaxLength = 9;
            txtMb= UiHelper.DodajRed(tbl, 2, "Matični broj *:");
            txtMb.MaxLength = 8;
            txtDelatnost = UiHelper.DodajRed(tbl, 3, "Delatnost:");
            txtAdresa    = UiHelper.DodajRed(tbl, 4, "Adresa:");
            txtTelefon= UiHelper.DodajRed(tbl, 5, "Telefon:");
            txtEmail= UiHelper.DodajRed(tbl, 6, "Email:");
            cmbStatus= UiHelper.DodajComboRed(tbl, 7, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "BLOKIRAN" });
            cmbStatus.SelectedIndex = 0;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 8);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
        }
    }
}
