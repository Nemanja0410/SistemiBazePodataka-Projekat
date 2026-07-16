using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class FazeObradeForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private DataGridView dgvFaze = null!;
        private Button       btnDodajFazu = null!, btnIzmeniFazu = null!, btnZatvori = null!;

        private void InitializeComponent()
        {
            this.Text            = $"Faze obrade — {_steta.BrojStete}";
            this.Size            = new Size(900, 460);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = UiHelper.PozadinaForm;

            var naslov = UiHelper.NapraviNaslov($"📋  Faze obrade — {_steta.BrojStete}");

            dgvFaze = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvFaze);
            dgvFaze.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Br.",               FillWeight = 5  });
            dgvFaze.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Naziv faze",        FillWeight = 22 });
            dgvFaze.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Datum početka",     FillWeight = 13 });
            dgvFaze.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Datum završetka",   FillWeight = 13 });
            dgvFaze.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Odgovorno lice",    FillWeight = 20 });
            dgvFaze.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Odluka",            FillWeight = 14 });
            dgvFaze.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dokumentacija",     FillWeight = 20 });
            dgvFaze.SelectionChanged += (s, e) => btnIzmeniFazu.Enabled = odabranaFaza() != null;

            var pnlBtn = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 46,
                BackColor = Color.White,
                Padding   = new Padding(8, 8, 8, 0)
            };
            btnDodajFazu  = UiHelper.NapraviDugme("➕  Dodaj fazu",  UiHelper.Zelena,      130);
            btnIzmeniFazu = UiHelper.NapraviDugme("✏️  Izmeni fazu", UiHelper.PlavaSvetla, 130);
            btnZatvori    = UiHelper.NapraviDugme("Zatvori",         UiHelper.Siva,        90);
            btnDodajFazu.Location  = new Point(8, 8);
            btnIzmeniFazu.Location = new Point(146, 8);
            btnZatvori.Location    = new Point(284, 8);
            btnIzmeniFazu.Enabled  = false;

            btnDodajFazu.Click  += BtnDodajFazu_Click;
            btnIzmeniFazu.Click += BtnIzmeniFazu_Click;
            btnZatvori.Click    += (s, e) => Close();

            pnlBtn.Controls.AddRange(new Control[] { btnDodajFazu, btnIzmeniFazu, btnZatvori });

            var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 8, 4) };
            pnlGrid.Controls.Add(dgvFaze);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(naslov);
        }
    }
}
