using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class UpravljanjeNalozimaForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            var naslov = UiHelper.NapraviNaslov("🔑  Upravljanje nalozima");

            var pnlT = new Panel { BackColor = Color.White, Dock = DockStyle.Top, Height = 50, AutoScroll = true };

            cmbStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(8, 12), Width = 190 };
            cmbStatus.Items.AddRange(new[] { "SVE", "NA_CEKANJU", "ODOBREN", "ODBIJEN", "ZAKLJUCAN" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += cmbStatus_SelectedIndexChanged;

            int bx = 207;
            btnOdobri= NB("✔  Odobri",     UiHelper.Zelena,      95,  ref bx);
            btnOdbij= NB("✖  Odbij",      UiHelper.Crvena,      90,  ref bx);
            btnZakljucaj = NB("🔒  Zaključaj",  UiHelper.Narandzasta, 105, ref bx);
            btnOtkljucaj = NB("🔓  Otključaj",  UiHelper.PlavaSvetla, 105, ref bx);
            btnResetuj   = NB("🔑  Reset",      UiHelper.Siva,        95,  ref bx);
            btnObrisi= NB("🗑️  Obriši",     UiHelper.Crvena,      95,  ref bx);
            btnOsvezi= NB("🔄  Osveži",     UiHelper.Siva,        100, ref bx);
            lblBroj= new Label { AutoSize = true, Location = new Point(bx + 6, 16), ForeColor = UiHelper.Siva, Text = "Ukupno: 0" };

            btnOdobri.Click    += btnOdobri_Click;
            btnOdbij.Click     += btnOdbij_Click;
            btnZakljucaj.Click += btnZakljucaj_Click;
            btnOtkljucaj.Click += btnOtkljucaj_Click;
            btnResetuj.Click   += btnResetuj_Click;
            btnObrisi.Click    += btnObrisi_Click;
            btnOsvezi.Click    += btnOsvezi_Click;

            pnlT.Controls.AddRange(new Control[] { cmbStatus, btnOdobri, btnOdbij, btnZakljucaj, btnOtkljucaj, btnResetuj, btnObrisi, btnOsvezi, lblBroj });

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 700, BorderStyle = BorderStyle.None };

            dgvNalozi = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvNalozi);
            dgvNalozi.Columns.Add(new DataGridViewTextBoxColumn { Name = "nColId",       HeaderText = "ID",              Visible = false });
            dgvNalozi.Columns.Add(new DataGridViewTextBoxColumn { Name = "nColKor",      HeaderText = "Korisničko ime",  FillWeight = 18, MinimumWidth = 130 });
            dgvNalozi.Columns.Add(new DataGridViewTextBoxColumn { Name = "nColZap",      HeaderText = "Zaposleni",       FillWeight = 22, MinimumWidth = 120 });
            dgvNalozi.Columns.Add(new DataGridViewTextBoxColumn { Name = "nColUloga",    HeaderText = "Uloga",           FillWeight = 12, MinimumWidth = 90  });
            dgvNalozi.Columns.Add(new DataGridViewTextBoxColumn { Name = "nColStatus",   HeaderText = "Status",          FillWeight = 12, MinimumWidth = 90  });
            dgvNalozi.Columns.Add(new DataGridViewTextBoxColumn { Name = "nColNeusp",    HeaderText = "Neuspeš.",        FillWeight = 8,  MinimumWidth = 80  });
            dgvNalozi.Columns.Add(new DataGridViewTextBoxColumn { Name = "nColReg",      HeaderText = "Registrovan",     FillWeight = 13, MinimumWidth = 100 });
            dgvNalozi.Columns.Add(new DataGridViewTextBoxColumn { Name = "nColPrijava",  HeaderText = "Zadnja prijava",  FillWeight = 15, MinimumWidth = 120 });
            dgvNalozi.SelectionChanged += dgvNalozi_SelectionChanged;
            dgvNalozi.Sorted += dgvNalozi_Sorted;
            split.Panel1.Controls.Add(dgvNalozi);
            split.Panel1.Padding = new Padding(8, 4, 4, 8);

            var pnlD = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16) };
            lblDetaljiNaziv = new Label { Dock = DockStyle.Top, Height = 36, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = UiHelper.Plava, Text = "Detalji naloga" };
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
            this.Size= new Size(1150, 680);
            this.MinimumSize = new Size(850, 480);
            this.Text= "Upravljanje nalozima";
            this.ResumeLayout(false);
        }

        private Button NB(string t, Color b, int w, ref int x)
        {
            var btn = UiHelper.NapraviDugme(t, b, w);
            btn.Location = new Point(x, 10);
            x += w + 6;
            return btn;
        }

        private DataGridView dgvNalozi = null!;
        private ComboBox     cmbStatus = null!;
        private Button       btnOdobri = null!, btnOdbij = null!, btnZakljucaj = null!, btnOtkljucaj = null!, btnResetuj = null!, btnObrisi = null!, btnOsvezi = null!;
        private Label        lblBroj = null!, lblDetaljiNaziv = null!;
        private RichTextBox  rtbDetalji = null!;
    }
}
