using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class FazeObradeForma : Form
    {
        private readonly StetaBasic _steta;
        private DataGridView dgvFaze = null!;
        private Button       btnDodajFazu = null!, btnIzmeniFazu = null!, btnZatvori = null!;

        public FazeObradeForma(StetaBasic st)
        {
            _steta = st;
            InitializeComponent();
            UcitajFaze();
        }

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

        private void UcitajFaze()
        {
            dgvFaze.Rows.Clear();
            foreach (var f in _steta.FazeObrade)
            {
                string zav = f.DatumZavrsetka.HasValue
                    ? f.DatumZavrsetka.Value.ToString("dd.MM.yyyy") : "—";

                int r = dgvFaze.Rows.Add(
                    f.RedniBrojFaze,
                    f.NazivFaze,
                    f.DatumPocetka.ToString("dd.MM.yyyy"),
                    zav,
                    f.OdgovornoLiceIme ?? "/",
                    f.Odluka ?? "/",
                    f.Dokumentacija ?? "/");

                if (!string.IsNullOrEmpty(f.Odluka))
                    dgvFaze.Rows[r].Cells[5].Style.ForeColor = UiHelper.StatusBoja(f.Odluka);

                dgvFaze.Rows[r].Tag = f;
            }
            btnIzmeniFazu.Enabled = false;
        }

        private FazaObradeBasic? odabranaFaza() =>
            dgvFaze.CurrentRow?.Tag as FazaObradeBasic;

        private void BtnDodajFazu_Click(object? sender, EventArgs e)
        {
            var f = new DodajFazuForma(_steta.StetaId, _steta.FazeObrade.Count + 1);
            if (f.ShowDialog() == DialogResult.OK) OsveziFaze();
        }

        private void BtnIzmeniFazu_Click(object? sender, EventArgs e)
        {
            var faza = odabranaFaza();
            if (faza == null) { MessageBox.Show("Izaberite fazu.", "Info"); return; }
            var f = new DodajFazuForma(faza);
            if (f.ShowDialog() == DialogResult.OK) OsveziFaze();
        }

        private void OsveziFaze()
        {
            var osvezen = DTOManager.vratiStetu(_steta.StetaId);
            _steta.FazeObrade.Clear();
            foreach (var faza in osvezen.FazeObrade)
                _steta.FazeObrade.Add(faza);
            UcitajFaze();
        }
    }
}
