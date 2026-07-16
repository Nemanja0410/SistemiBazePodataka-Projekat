using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class OblastiProceneForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private ListBox  lstOblasti = null!;
        private ComboBox cmbNovaOblast = null!;
        private Label    lblSveDodato = null!;
        private Button   btnDodaj = null!, btnZatvori = null!;
        private Panel    pnlUnos = null!;

        private void InitializeComponent()
        {
            this.Text            = $"Oblasti procene — {_procenitelj.Ime} {_procenitelj.Prezime}";
            this.Size            = new Size(420, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = UiHelper.PozadinaForm;

            var naslov = UiHelper.NapraviNaslov("📚  Oblasti procene");

            var lblHint = new Label
            {
                Text = "Dupli klik na oblast za uklanjanje.",
                Dock = DockStyle.Top, Height = 22,
                ForeColor = UiHelper.Siva, Padding = new Padding(8, 4, 0, 0)
            };

            lstOblasti = new ListBox { Dock = DockStyle.Fill };
            lstOblasti.DoubleClick += LstOblasti_DoubleClick;

            pnlUnos = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.White, Padding = new Padding(8) };
            cmbNovaOblast = new ComboBox { Location = new Point(8, 14), Size = new Size(300, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            btnDodaj = UiHelper.NapraviDugme("➕ Dodaj", UiHelper.Zelena, 90);
            btnDodaj.Location = new Point(314, 13);
            btnDodaj.Click += BtnDodaj_Click;
            lblSveDodato = new Label
            {
                Text = "Sve oblasti su već dodeljene.", AutoSize = true,
                Location = new Point(8, 18), ForeColor = UiHelper.Siva, Visible = false
            };
            pnlUnos.Controls.Add(cmbNovaOblast);
            pnlUnos.Controls.Add(btnDodaj);
            pnlUnos.Controls.Add(lblSveDodato);

            var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White, Padding = new Padding(8, 8, 8, 0) };
            btnZatvori = UiHelper.NapraviDugme("Zatvori", UiHelper.Siva, 90);
            btnZatvori.Location = new Point(8, 8);
            btnZatvori.Click += (s, e) => Close();
            pnlBtn.Controls.Add(btnZatvori);

            var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 8, 4) };
            pnlGrid.Controls.Add(lstOblasti);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(pnlUnos);
            this.Controls.Add(lblHint);
            this.Controls.Add(naslov);
        }
    }
}
