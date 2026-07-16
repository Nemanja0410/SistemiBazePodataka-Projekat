using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class IzmeniOsobljeForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox  txtIme = null!, txtPrezime = null!, txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private ComboBox cmbStatus = null!, cmbTipPravnika = null!;
        private TextBox  txtBarBroj = null!, txtBrojLicence = null!, txtSpecijalizacija = null!;
        private TableLayoutPanel tbl = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

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
            bool jeLekar       = _osoblje.TipOsoblja == "LEKAR";

            tbl = UiHelper.NapraviLayout(11);

            txtIme     = UiHelper.DodajRed(tbl, 0, "Ime *:");
            txtPrezime = UiHelper.DodajRed(tbl, 1, "Prezime *:");
            txtAdresa  = UiHelper.DodajRed(tbl, 2, "Adresa:");
            txtTelefon = UiHelper.DodajRed(tbl, 3, "Telefon:");
            txtEmail   = UiHelper.DodajRed(tbl, 4, "Email:");
            cmbStatus  = UiHelper.DodajComboRed(tbl, 5, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "SUSPENDOVAN" });

            cmbTipPravnika = UiHelper.DodajComboRed(tbl, 6, "Tip pravnika:");
            cmbTipPravnika.Items.AddRange(new[] { "INTERNI", "EKSTERNI" });
            txtBarBroj     = UiHelper.DodajRed(tbl, 7, "Broj advokata:");
            txtBrojLicence = UiHelper.DodajRed(tbl, 8, "Broj licence:");
            txtSpecijalizacija = UiHelper.DodajRed(tbl, 9, "Specijalizacija:");

            cmbTipPravnika.Visible = jePravnik;
            txtBarBroj.Visible     = jePravnik;
            tbl.GetControlFromPosition(0, 6)!.Visible = jePravnik;
            tbl.GetControlFromPosition(0, 7)!.Visible = jePravnik;
            tbl.RowStyles[6].Height = jePravnik ? 36 : 0;
            tbl.RowStyles[7].Height = jePravnik ? 36 : 0;

            bool imaLicencu = jeProcenitelj || jeLekar;
            txtBrojLicence.Visible = imaLicencu;
            tbl.GetControlFromPosition(0, 8)!.Visible = imaLicencu;
            tbl.RowStyles[8].Height = imaLicencu ? 36 : 0;

            txtSpecijalizacija.Visible = jeLekar;
            tbl.GetControlFromPosition(0, 9)!.Visible = jeLekar;
            tbl.RowStyles[9].Height = jeLekar ? 36 : 0;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 10);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);

            int sadrzajVisina = 0;
            foreach (RowStyle rs in tbl.RowStyles) sadrzajVisina += (int)rs.Height;
            this.ClientSize = new Size(450, sadrzajVisina + tbl.Padding.Top + tbl.Padding.Bottom);
        }
    }
}
