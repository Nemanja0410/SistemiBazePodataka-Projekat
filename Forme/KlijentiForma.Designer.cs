using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class KlijentiForma
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Naslov
            var naslov = UiHelper.NapraviNaslov("👤  Upravljanje klijentima");

            // Toolbar
            var pnlToolbar = new Panel
            {
                BackColor = Color.White, Dock = DockStyle.Top,
                Height = 50, Padding = new Padding(8, 10, 8, 0)
            };

            var lblPretraga = new Label { Text = "Pretraga:", AutoSize = true, Location = new Point(8, 13), Padding = new Padding(0, 0, 4, 0) };
            txtPretraga = new TextBox { Location = new Point(76, 10), Width = 210, PlaceholderText = "Naziv klijenta..." };
            txtPretraga.TextChanged += txtPretraga_TextChanged;

            var lblTip = new Label { Text = "Tip:", AutoSize = true, Location = new Point(299, 13), Padding = new Padding(0, 0, 4, 0) };
            cmbTip = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(330, 10), Width = 190 };
            cmbTip.Items.AddRange(new[] { "SVE", "FIZICKO_LICE", "PRAVNO_LICE", "JAVNA_INSTITUCIJA" });
            cmbTip.SelectedIndex = 0;
            cmbTip.SelectedIndexChanged += cmbTip_SelectedIndexChanged;

            int bx = 532;
            btnDodajFizicko    = NB("➕ Fizičko lice",  UiHelper.Zelena,      130, ref bx);
            btnDodajPravno     = NB("➕ Pravno lice",   UiHelper.Zelena,      120, ref bx);
            btnDodajInstituciju= NB("➕ Institucija",   UiHelper.Zelena,      110, ref bx);
            btnIzmeni          = NB("✏️  Izmeni",       UiHelper.PlavaSvetla, 95,  ref bx);
            btnObrisi          = NB("🗑️  Obriši",       UiHelper.Crvena,      90,  ref bx);
            btnOsvezi          = NB("🔄  Osveži",       UiHelper.Siva,        88,  ref bx);
            lblBroj            = new Label { AutoSize = true, ForeColor = UiHelper.Siva, Location = new Point(bx + 6, 13), Text = "Ukupno: 0" };

            btnDodajFizicko.Click    += btnDodajFizicko_Click;
            btnDodajPravno.Click     += btnDodajPravno_Click;
            btnDodajInstituciju.Click += btnDodajInstituciju_Click;
            btnIzmeni.Click          += btnIzmeni_Click;
            btnObrisi.Click          += btnObrisi_Click;
            btnOsvezi.Click          += btnOsvezi_Click;

            pnlToolbar.Controls.AddRange(new Control[]
            {
                txtPretraga,lblPretraga, cmbTip, lblTip,
                btnDodajFizicko, btnDodajPravno, btnDodajInstituciju,
                btnIzmeni, btnObrisi, btnOsvezi, lblBroj
            });

            // Split container
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill, SplitterDistance = 680,
                BorderStyle = BorderStyle.None
            };

            // Grid
            dgvKlijenti = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvKlijenti);
            dgvKlijenti.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",      HeaderText = "ID",                  Visible = false });
            dgvKlijenti.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNaziv",   HeaderText = "Naziv / Ime",          FillWeight = 28 });
            dgvKlijenti.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTip",     HeaderText = "Tip klijenta",         FillWeight = 18 });
            dgvKlijenti.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTelefon", HeaderText = "Telefon",              FillWeight = 15 });
            dgvKlijenti.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmail",   HeaderText = "Email",                FillWeight = 24 });
            dgvKlijenti.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus",  HeaderText = "Status",               FillWeight = 10 });
            dgvKlijenti.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDat",     HeaderText = "Registrovan",          FillWeight = 12 });
            dgvKlijenti.CellDoubleClick  += dgvKlijenti_CellDoubleClick;
            dgvKlijenti.SelectionChanged += dgvKlijenti_SelectionChanged;
            split.Panel1.Controls.Add(dgvKlijenti);
            split.Panel1.Padding = new Padding(8, 4, 4, 8);

            // Detalji
            var pnlDet = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16) };
            lblDetaljiNaziv = new Label
            {
                Dock = DockStyle.Top, Height = 36,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = UiHelper.Plava, Text = "Detalji klijenta"
            };
            rtbDetalji = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5F), BackColor = Color.White
            };
            pnlDet.Controls.Add(rtbDetalji);
            pnlDet.Controls.Add(lblDetaljiNaziv);
            split.Panel2.Controls.Add(pnlDet);
            split.Panel2.Padding = new Padding(4, 4, 8, 8);

            this.Controls.Add(split);
            this.Controls.Add(pnlToolbar);
            this.Controls.Add(naslov);

            // Forma
            this.BackColor = UiHelper.PozadinaForm;
            this.Font      = new Font("Segoe UI", 9F);
            this.Size      = new Size(1150, 700);
            this.Text      = "Klijenti";

            this.ResumeLayout(false);
        }

        private Button NB(string t, Color b, int w, ref int x)
        {
            var btn = UiHelper.NapraviDugme(t, b, w);
            btn.Location = new Point(x, 10);
            x += w + 6;
            return btn;
        }

        private DataGridView dgvKlijenti;
        private TextBox      txtPretraga;
        private ComboBox     cmbTip;
        private Button       btnDodajFizicko, btnDodajPravno, btnDodajInstituciju;
        private Button       btnIzmeni, btnObrisi, btnOsvezi;
        private Label        lblBroj, lblDetaljiNaziv;
        private RichTextBox  rtbDetalji;
    }
}
