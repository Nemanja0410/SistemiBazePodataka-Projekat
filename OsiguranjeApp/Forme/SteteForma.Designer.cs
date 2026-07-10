using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class SteteForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            var naslov = UiHelper.NapraviNaslov("⚠️  Upravljanje štetama");

            var pnlT = new Panel { BackColor = Color.White, Dock = DockStyle.Top, Height = 50, AutoScroll = true };

            cmbVrsta = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(8, 12), Width = 180 };
            cmbVrsta.Items.AddRange(new[] { "SVE","AUTO","ZDRAVSTVENA","IMOVINSKA","PUTNA","ZIVOTNA","OSTALO" });
            cmbVrsta.SelectedIndex = 0;
            cmbVrsta.SelectedIndexChanged += cmbVrsta_SelectedIndexChanged;

            cmbStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(196, 12), Width = 190 };
            cmbStatus.Items.AddRange(new[] { "SVE","PRIJAVLJENA","U_OBRADI","U_PROCENI","ODOBRENA","ODBIJENA","ISPLACENA","ZATVORENA" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += cmbStatus_SelectedIndexChanged;

            int bx = 396;
            btnDodaj  = NB("➕  Dodaj",  UiHelper.Zelena,      100, ref bx);
            btnIzmeni = NB("✏️  Izmeni", UiHelper.PlavaSvetla, 95,  ref bx);
            btnObrisi = NB("🗑️  Obriši", UiHelper.Crvena,      90,  ref bx);
            btnFaze   = NB("📋  Faze",   UiHelper.Narandzasta, 85,  ref bx);
            btnFotografije = NB("📷  Foto", UiHelper.PlavaSvetla, 85, ref bx);
            btnOsvezi = NB("🔄  Osveži", UiHelper.Siva,        85,  ref bx);
            lblBroj   = new Label { AutoSize = true, Location = new Point(bx + 6, 16), ForeColor = UiHelper.Siva, Text = "Ukupno: 0" };

            btnDodaj.Click  += btnDodaj_Click;
            btnIzmeni.Click += btnIzmeni_Click;
            btnObrisi.Click += btnObrisi_Click;
            btnFaze.Click   += btnFaze_Click;
            btnFotografije.Click += btnFotografije_Click;
            btnOsvezi.Click += btnOsvezi_Click;

            pnlT.Controls.AddRange(new Control[] { cmbVrsta, cmbStatus, btnDodaj, btnIzmeni, btnObrisi, btnFaze, btnFotografije, btnOsvezi, lblBroj });

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 660, BorderStyle = BorderStyle.None };

            dgvStete = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvStete);
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColId",      HeaderText = "ID",             Visible = false });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColBroj",    HeaderText = "Broj štete",     FillWeight = 20, MinimumWidth = 110 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColVrsta",   HeaderText = "Vrsta",          FillWeight = 14, MinimumWidth = 90  });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColPod",     HeaderText = "Podnosilac",     FillWeight = 22, MinimumWidth = 120 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColPolisa",  HeaderText = "Polisa",         FillWeight = 18, MinimumWidth = 110 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColIznos",   HeaderText = "Procenjeni iznos", FillWeight = 14, MinimumWidth = 130 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColStatus",  HeaderText = "Status",         FillWeight = 12, MinimumWidth = 90  });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColDat",     HeaderText = "Datum prijave",  FillWeight = 12, MinimumWidth = 110 });
            dgvStete.CellDoubleClick  += dgvStete_CellDoubleClick;
            dgvStete.SelectionChanged += dgvStete_SelectionChanged;
            dgvStete.CellFormatting   += dgvStete_CellFormatting;
            split.Panel1.Controls.Add(dgvStete);
            split.Panel1.Padding = new Padding(8, 4, 4, 8);

            var pnlD = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16) };
            lblDetaljiNaziv = new Label { Dock = DockStyle.Top, Height = 36, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = UiHelper.Plava, Text = "Detalji štete" };
            rtbDetalji = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9.5F), BackColor = Color.White };
            pnlD.Controls.Add(rtbDetalji);
            pnlD.Controls.Add(lblDetaljiNaziv);
            split.Panel2.Controls.Add(pnlD);
            split.Panel2.Padding = new Padding(4, 4, 8, 8);

            this.Controls.Add(split);
            this.Controls.Add(pnlT);
            this.Controls.Add(naslov);
            this.BackColor = UiHelper.PozadinaForm;
            this.Font      = new Font("Segoe UI", 9F);
            this.Size      = new Size(1150, 720);
            this.MinimumSize = new Size(900, 500);
            this.Text      = "Štete";
            this.ResumeLayout(false);
        }

        private Button NB(string t, Color b, int w, ref int x)
        {
            var btn = UiHelper.NapraviDugme(t, b, w);
            btn.Location = new Point(x, 10);
            x += w + 6;
            return btn;
        }

        private DataGridView dgvStete = null!;
        private ComboBox     cmbVrsta = null!, cmbStatus = null!;
        private Button       btnDodaj = null!, btnIzmeni = null!, btnObrisi = null!, btnFaze = null!, btnFotografije = null!, btnOsvezi = null!;
        private Label        lblBroj = null!, lblDetaljiNaziv = null!;
        private RichTextBox  rtbDetalji = null!;
    }
}
