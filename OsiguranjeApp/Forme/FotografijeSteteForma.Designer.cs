using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class FotografijeSteteForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private DataGridView dgvFoto = null!;
        private TextBox txtIzabranFajl = null!, txtOpis = null!;
        private Button btnIzaberi = null!, btnDodaj = null!, btnObrisi = null!, btnOtvori = null!, btnZatvori = null!;

        private void InitializeComponent()
        {
            this.Text          = $"Fotografije — {_brojStete}";
            this.Size          = new Size(700, 460);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font          = new Font("Segoe UI", 9f);
            this.BackColor     = UiHelper.PozadinaForm;

            var naslov = UiHelper.NapraviNaslov($"📷  Fotografije — {_brojStete}");

            dgvFoto = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvFoto);
            dgvFoto.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false });
            dgvFoto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fajl", FillWeight = 50 });
            dgvFoto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Opis", FillWeight = 30 });
            dgvFoto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dodato", FillWeight = 20 });
            dgvFoto.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OtvoriIzabranu(); };

            var pnlUnos = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 76, ColumnCount = 3, Padding = new Padding(8) };
            pnlUnos.BackColor = Color.White;
            pnlUnos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            pnlUnos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            pnlUnos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            pnlUnos.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            pnlUnos.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

            pnlUnos.Controls.Add(new Label { Text = "Fajl (JPG, PNG, WEBP) *", AutoSize = true }, 0, 0);
            pnlUnos.Controls.Add(new Label { Text = "Opis", AutoSize = true }, 1, 0);

            var pnlFajl = new Panel { Dock = DockStyle.Fill };
            txtIzabranFajl = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            btnIzaberi = UiHelper.NapraviDugme("📁", UiHelper.Siva, 34);
            btnIzaberi.Dock = DockStyle.Right;
            btnIzaberi.Click += BtnIzaberi_Click;
            pnlFajl.Controls.Add(txtIzabranFajl);
            pnlFajl.Controls.Add(btnIzaberi);

            txtOpis = new TextBox { Dock = DockStyle.Fill };
            pnlUnos.Controls.Add(pnlFajl, 0, 1);
            pnlUnos.Controls.Add(txtOpis, 1, 1);

            btnDodaj = UiHelper.NapraviDugme("➕  Dodaj", UiHelper.Zelena, 95);
            btnDodaj.Dock = DockStyle.Bottom;
            pnlUnos.Controls.Add(btnDodaj, 2, 1);
            btnDodaj.Click += BtnDodaj_Click;

            var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.White, Padding = new Padding(8, 6, 8, 0) };
            btnOtvori  = UiHelper.NapraviDugme("🖼️  Otvori", UiHelper.PlavaSvetla, 100);
            btnObrisi  = UiHelper.NapraviDugme("🗑️  Obriši izabranu", UiHelper.Crvena, 150);
            btnZatvori = UiHelper.NapraviDugme("Zatvori", UiHelper.Siva, 90);
            btnOtvori.Location  = new Point(8, 4);
            btnObrisi.Location  = new Point(114, 4);
            btnZatvori.Location = new Point(272, 4);
            btnOtvori.Click  += (s, e) => OtvoriIzabranu();
            btnObrisi.Click  += BtnObrisi_Click;
            btnZatvori.Click += (s, e) => Close();
            pnlBtn.Controls.AddRange(new Control[] { btnOtvori, btnObrisi, btnZatvori });

            var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 8, 4) };
            pnlGrid.Controls.Add(dgvFoto);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(pnlUnos);
            this.Controls.Add(naslov);
        }
    }
}
