using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class PoliseForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            var naslov = UiHelper.NapraviNaslov("📋  Polise osiguranja");

            var pnlT = new Panel { BackColor = Color.White, Dock = DockStyle.Top, Height = 50, AutoScroll = true };

            cmbTip = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(8, 12), Width = 210, DropDownWidth = 210 };
            cmbTip.Items.AddRange(new[] { "SVE","ZIVOTNO","ZDRAVSTVENO","IMOVINSKO","AUTO","PUTNO","POLJOPRIVREDNO","ODGOVORNOST","SPECIJALIZOVANO" });
            cmbTip.SelectedIndex = 0;
            cmbTip.SelectedIndexChanged += cmbTip_SelectedIndexChanged;

            cmbStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(226, 12), Width = 170 };
            cmbStatus.Items.AddRange(new[] { "SVE","AKTIVNA","ISTEKLA","RASKINUTA","MIROVANJE","OBNOVLJENA" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += cmbStatus_SelectedIndexChanged;

            int bx = 404;
            btnDodaj    = NB("➕  Dodaj",     UiHelper.Zelena,      100, ref bx);
            btnIzmeni   = NB("✏️  Izmeni",    UiHelper.PlavaSvetla, 95,  ref bx);
            btnObrisi   = NB("🗑️  Obriši",    UiHelper.Crvena,      90,  ref bx);
            btnIstorija = NB("🕒  Istorija",  UiHelper.Siva,        95,  ref bx);
            btnOsvezi   = NB("🔄  Osveži",    UiHelper.Siva,        88,  ref bx);
            lblBroj     = new Label { AutoSize = true, Location = new Point(bx + 6, 16), ForeColor = UiHelper.Siva, Text = "Ukupno: 0" };

            btnDodaj.Click    += btnDodaj_Click;
            btnIzmeni.Click   += btnIzmeni_Click;
            btnObrisi.Click   += btnObrisi_Click;
            btnIstorija.Click += btnIstorija_Click;
            btnOsvezi.Click   += btnOsvezi_Click;

            pnlT.Controls.AddRange(new Control[] { cmbTip, cmbStatus, btnDodaj, btnIzmeni, btnObrisi, btnIstorija, btnOsvezi, lblBroj });

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 680, BorderStyle = BorderStyle.None };

            dgvPolise = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvPolise);
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColId",      HeaderText = "ID",       Visible = false });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColBroj",    HeaderText = "Broj polise",   FillWeight = 20, MinimumWidth = 110 });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColTip",     HeaderText = "Tip",           FillWeight = 16, MinimumWidth = 90  });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColUgovarac",HeaderText = "Ugovarač",      FillWeight = 22, MinimumWidth = 120 });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColPremija", HeaderText = "Premija",       FillWeight = 14, MinimumWidth = 90  });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColStatus",  HeaderText = "Status",        FillWeight = 11, MinimumWidth = 80  });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColOd",      HeaderText = "Važi od",       FillWeight = 10, MinimumWidth = 80  });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColDo",      HeaderText = "Važi do",       FillWeight = 10, MinimumWidth = 80  });
            dgvPolise.CellDoubleClick  += dgvPolise_CellDoubleClick;
            dgvPolise.SelectionChanged += dgvPolise_SelectionChanged;
            split.Panel1.Controls.Add(dgvPolise);
            split.Panel1.Padding = new Padding(8, 4, 4, 8);

            var pnlD = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16) };
            lblDetaljiNaziv = new Label { Dock = DockStyle.Top, Height = 36, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = UiHelper.Plava, Text = "Detalji polise" };
            rtbDetalji = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9.5F), BackColor = Color.White };
            pnlD.Controls.Add(rtbDetalji);
            pnlD.Controls.Add(lblDetaljiNaziv);
            split.Panel2.Controls.Add(pnlD);
            split.Panel2.Padding = new Padding(4, 4, 8, 8);

            this.Controls.Add(split);
            this.Controls.Add(pnlT);
            this.Controls.Add(naslov);
            this.BackColor = UiHelper.PozadinaForm;
            this.Font= new Font("Segoe UI", 9F);
            this.Size= new Size(1150, 720);
            this.MinimumSize = new Size(850, 500);
            this.Text= "Polise osiguranja";
            this.ResumeLayout(false);
        }

        private Button NB(string t, Color b, int w, ref int x)
        {
            var btn = UiHelper.NapraviDugme(t, b, w);
            btn.Location = new Point(x, 10);
            x += w + 6;
            return btn;
        }

        private DataGridView dgvPolise = null!;
        private ComboBox     cmbTip = null!, cmbStatus = null!;
        private Button       btnDodaj = null!, btnIzmeni = null!, btnObrisi = null!, btnOsvezi = null!, btnIstorija = null!;
        private Label        lblBroj = null!, lblDetaljiNaziv = null!;
        private RichTextBox  rtbDetalji = null!;
    }
}
