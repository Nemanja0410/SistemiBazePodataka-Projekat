using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class IzmeniKlijentaForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox  txtNaziv = null!, txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private ComboBox cmbStatus = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        private DataGridView dgvKontakti = null!;
        private TextBox txtKoIme = null!, txtKoPrezime = null!, txtKoFunkcija = null!, txtKoTelefon = null!, txtKoEmail = null!;

        private void InitializeComponent()
        {
            this.Text            = $"Izmeni klijenta — {_klijent.Naziv}";
            this.Size            = _imaKontakte ? new Size(500, 560) : new Size(450, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(6);

            txtNaziv   = UiHelper.DodajRed(tbl, 0, "Naziv / Ime *:");
            txtAdresa  = UiHelper.DodajRed(tbl, 1, "Adresa:");
            txtTelefon = UiHelper.DodajRed(tbl, 2, "Telefon:");
            txtEmail   = UiHelper.DodajRed(tbl, 3, "Email:");
            cmbStatus  = UiHelper.DodajComboRed(tbl, 4, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "BLOKIRAN" });

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 5);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);

            if (_imaKontakte)
            {
                tbl.Height = 260;
                tbl.Dock = DockStyle.Top;
                DodajKontaktOsobeSekciju();
            }
        }
    }
}
