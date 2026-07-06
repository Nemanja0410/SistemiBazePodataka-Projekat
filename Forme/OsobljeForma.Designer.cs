using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class OsobljeForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            var naslov = UiHelper.NapraviNaslov("👥  Upravljanje osobljem");

            var pnlT = new Panel { BackColor = Color.White, Dock = DockStyle.Top, Height = 50, AutoScroll = true };

            cmbTip = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(8, 12), Width = 190 };
            cmbTip.Items.AddRange(new[] { "SVE","AGENT","PROCENITELJ","LEKAR","PRAVNIK","OSTALO" });
            cmbTip.SelectedIndex = 0;
            cmbTip.SelectedIndexChanged += cmbTip_SelectedIndexChanged;

            int bx = 207;
            btnDodajAgenta = NB("➕  Agent",   UiHelper.Zelena,      100, ref bx);
            btnDodajOstalo = NB("➕  Ostalo",  UiHelper.Zelena,      95,  ref bx);
            btnIzmeni= NB("✏️  Izmeni",  UiHelper.PlavaSvetla, 95,  ref bx);
            btnObrisi= NB("🗑️  Obriši",  UiHelper.Crvena,      90,  ref bx);
            btnOsvezi= NB("🔄  Osveži",  UiHelper.Siva,        85,  ref bx);
            lblBroj= new Label { AutoSize = true, Location = new Point(bx + 6, 16), ForeColor = UiHelper.Siva, Text = "Ukupno: 0" };

            btnDodajAgenta.Click += btnDodajAgenta_Click;
            btnDodajOstalo.Click += btnDodajOstalo_Click;
            btnIzmeni.Click+= btnIzmeni_Click;
            btnObrisi.Click+= btnObrisi_Click;
            btnOsvezi.Click+= btnOsvezi_Click;

            pnlT.Controls.AddRange(new Control[] { cmbTip, btnDodajAgenta, btnDodajOstalo, btnIzmeni, btnObrisi, btnOsvezi, lblBroj });

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 660, BorderStyle = BorderStyle.None };

            dgvOsoblje = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvOsoblje);
            dgvOsoblje.Columns.Add(new DataGridViewTextBoxColumn { Name = "oColId",     HeaderText = "ID",              Visible = false });
            dgvOsoblje.Columns.Add(new DataGridViewTextBoxColumn { Name = "oColIme",    HeaderText = "Ime i prezime",   FillWeight = 24, MinimumWidth = 130 });
            dgvOsoblje.Columns.Add(new DataGridViewTextBoxColumn { Name = "oColTip",    HeaderText = "Tip",             FillWeight = 14, MinimumWidth = 90  });
            dgvOsoblje.Columns.Add(new DataGridViewTextBoxColumn { Name = "oColTel",    HeaderText = "Telefon",         FillWeight = 15, MinimumWidth = 90  });
            dgvOsoblje.Columns.Add(new DataGridViewTextBoxColumn { Name = "oColEmail",  HeaderText = "Email",           FillWeight = 22, MinimumWidth = 140 });
            dgvOsoblje.Columns.Add(new DataGridViewTextBoxColumn { Name = "oColRegion", HeaderText = "Region",          FillWeight = 14, MinimumWidth = 90  });
            dgvOsoblje.Columns.Add(new DataGridViewTextBoxColumn { Name = "oColStatus", HeaderText = "Status",          FillWeight = 10, MinimumWidth = 80  });
            dgvOsoblje.Columns.Add(new DataGridViewTextBoxColumn { Name = "oColAng",    HeaderText = "Angažovan",       FillWeight = 12, MinimumWidth = 110 });
            dgvOsoblje.CellDoubleClick  += dgvOsoblje_CellDoubleClick;
            dgvOsoblje.SelectionChanged += dgvOsoblje_SelectionChanged;
            split.Panel1.Controls.Add(dgvOsoblje);
            split.Panel1.Padding = new Padding(8, 4, 4, 8);

            var pnlD = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16) };
            lblDetaljiNaziv = new Label { Dock = DockStyle.Top, Height = 36, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = UiHelper.Plava, Text = "Detalji zaposlenog" };
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
            this.Size= new Size(1100, 680);
            this.MinimumSize = new Size(850, 480);
            this.Text= "Osoblje";
            this.ResumeLayout(false);
        }

        private Button NB(string t, Color b, int w, ref int x)
        {
            var btn = UiHelper.NapraviDugme(t, b, w);
            btn.Location = new Point(x, 10);
            x += w + 6;
            return btn;
        }

        private DataGridView dgvOsoblje = null!;
        private ComboBox     cmbTip = null!;
        private Button       btnDodajAgenta = null!, btnDodajOstalo = null!, btnIzmeni = null!, btnObrisi = null!, btnOsvezi = null!;
        private Label        lblBroj = null!, lblDetaljiNaziv = null!;
        private RichTextBox  rtbDetalji = null!;
    }
}
