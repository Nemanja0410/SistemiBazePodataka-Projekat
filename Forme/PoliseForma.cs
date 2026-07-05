using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class PoliseForma : Form
    {
        public PoliseForma()
        {
            InitializeComponent();
            this.Load += (s, e) => this.BeginInvoke(new Action(ucitajPolise));
        }

        private void ucitajPolise()
        {
            try
            {
                var lista = DTOManager.vratiSvePolise(
                    cmbTip.SelectedItem?.ToString(),
                    cmbStatus.SelectedItem?.ToString());

                dgvPolise.SelectionChanged -= dgvPolise_SelectionChanged;
                dgvPolise.Rows.Clear();
                foreach (var p in lista)
                {
                    int r = dgvPolise.Rows.Add(
                        p.PolisaId, p.BrojPolise, p.TipOsiguranja,
                        p.UgovaracNaziv,
                        $"{p.OsnovnaPremija:N2} {p.Valuta}",
                        p.Status,
                        p.DatumPocetka.ToString("dd.MM.yyyy"),
                        p.DatumIsteka.ToString("dd.MM.yyyy"));

                    dgvPolise.Rows[r].Cells["pColStatus"].Style.ForeColor = UiHelper.StatusBoja(p.Status);
                    dgvPolise.Rows[r].Cells["pColStatus"].Style.Font =
                        new Font("Segoe UI", 8.5f, FontStyle.Bold);

                    if (p.Status == "AKTIVNA"
                        && p.DatumIsteka <= DateTime.Today.AddDays(30)
                        && p.DatumIsteka >= DateTime.Today)
                        dgvPolise.Rows[r].DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);

                    dgvPolise.Rows[r].Tag = p;
                }
                dgvPolise.SelectionChanged += dgvPolise_SelectionChanged;
                if (dgvPolise.Rows.Count > 0) dgvPolise.FirstDisplayedScrollingRowIndex = 0;
                lblBroj.Text = $"Ukupno: {lista.Count}";
                dgvPolise_SelectionChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška: " + ex.Message, "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private PolisaPregled? odabranaPolisa() =>
            dgvPolise.CurrentRow?.Tag as PolisaPregled;

        private void cmbTip_SelectedIndexChanged(object? sender, EventArgs e)    => ucitajPolise();
        private void cmbStatus_SelectedIndexChanged(object? sender, EventArgs e) => ucitajPolise();
        private void btnOsvezi_Click(object? sender, EventArgs e)                => ucitajPolise();

        private void dgvPolise_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnIzmeni_Click(sender, e);
        }

        private void btnDodaj_Click(object? sender, EventArgs e)
        {
            var f = new DodajPolisaForma();
            if (f.ShowDialog() == DialogResult.OK) ucitajPolise();
        }

        private void btnIzmeni_Click(object? sender, EventArgs e)
        {
            var p = odabranaPolisa();
            if (p == null) { MessageBox.Show("Izaberite polisu.", "Info"); return; }
            var f = new IzmeniPolisaForma(DTOManager.vratiPolisu(p.PolisaId));
            if (f.ShowDialog() == DialogResult.OK) ucitajPolise();
        }

        private void btnObrisi_Click(object? sender, EventArgs e)
        {
            var p = odabranaPolisa();
            if (p == null) { MessageBox.Show("Izaberite polisu.", "Info"); return; }
            if (MessageBox.Show($"Obrisati polisu {p.BrojPolise}?", "Potvrda",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DTOManager.obrisiPolisu(p.PolisaId);
                ucitajPolise();
            }
        }

        private void dgvPolise_SelectionChanged(object? sender, EventArgs e)
        {
            var p = odabranaPolisa();
            if (p == null) return;

            lblDetaljiNaziv.Text = p.BrojPolise;
            rtbDetalji.Clear();
            rtbDetalji.AppendText($"Tip:            {p.TipOsiguranja}\n");
            rtbDetalji.AppendText($"Status:         {p.Status}\n");
            rtbDetalji.AppendText($"Ugovarač:       {p.UgovaracNaziv}\n");
            rtbDetalji.AppendText($"Agent:          {p.AgentIme ?? "/"}\n\n");
            rtbDetalji.AppendText("── Finansije ─────────────────────────\n");
            rtbDetalji.AppendText($"Premija:        {p.OsnovnaPremija:N2} {p.Valuta}\n");
            rtbDetalji.AppendText($"Način plaćanja: {p.NacinPlacanja}\n\n");
            rtbDetalji.AppendText("── Trajanje ──────────────────────────\n");
            rtbDetalji.AppendText($"Važi od:        {p.DatumPocetka:dd.MM.yyyy}\n");
            rtbDetalji.AppendText($"Važi do:        {p.DatumIsteka:dd.MM.yyyy}\n");
            int dana = (int)(p.DatumIsteka - DateTime.Today).TotalDays;
            if (p.Status == "AKTIVNA")
                rtbDetalji.AppendText($"Preostalo:      {dana} dana\n");

            try
            {
                var pb = DTOManager.vratiPolisu(p.PolisaId);
                if (pb.DodatnaPokrića.Count > 0)
                {
                    rtbDetalji.AppendText($"\n── Dodatna pokrića ({pb.DodatnaPokrića.Count}) ──────────\n");
                    foreach (var dp in pb.DodatnaPokrića)
                        rtbDetalji.AppendText($"• {dp.Naziv} — +{dp.DodatnaPremija:N2} RSD\n");
                }
            }
            catch { }
        }
    }

    partial class PoliseForma
    {
        private System.ComponentModel.IContainer? components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            var naslov = UiHelper.NapraviNaslov("📋  Polise osiguranja");

            var pnlT = new Panel { BackColor = Color.White, Dock = DockStyle.Top, Height = 50 };

            cmbTip = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(8, 12), Width = 210, DropDownWidth = 210 };
            cmbTip.Items.AddRange(new[] { "SVE","ZIVOTNO","ZDRAVSTVENO","IMOVINSKO","AUTO","PUTNO","POLJOPRIVREDNO","ODGOVORNOST","SPECIJALIZOVANO" });
            cmbTip.SelectedIndex = 0;
            cmbTip.SelectedIndexChanged += cmbTip_SelectedIndexChanged;

            cmbStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(226, 12), Width = 170 };
            cmbStatus.Items.AddRange(new[] { "SVE","AKTIVNA","ISTEKLA","RASKINUTA","MIROVANJE","OBNOVLJENA" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += cmbStatus_SelectedIndexChanged;

            int bx = 404;
            btnDodaj  = NB("➕  Dodaj",  UiHelper.Zelena,      100, ref bx);
            btnIzmeni = NB("✏️  Izmeni", UiHelper.PlavaSvetla, 95,  ref bx);
            btnObrisi = NB("🗑️  Obriši", UiHelper.Crvena,      90,  ref bx);
            btnOsvezi = NB("🔄  Osveži", UiHelper.Siva,        88,  ref bx);
            lblBroj   = new Label { AutoSize = true, Location = new Point(bx + 6, 16), ForeColor = UiHelper.Siva, Text = "Ukupno: 0" };

            btnDodaj.Click  += btnDodaj_Click;
            btnIzmeni.Click += btnIzmeni_Click;
            btnObrisi.Click += btnObrisi_Click;
            btnOsvezi.Click += btnOsvezi_Click;

            pnlT.Controls.AddRange(new Control[] { cmbTip, cmbStatus, btnDodaj, btnIzmeni, btnObrisi, btnOsvezi, lblBroj });

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 680, BorderStyle = BorderStyle.None };

            dgvPolise = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvPolise);
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColId",      HeaderText = "ID",       Visible = false });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColBroj",    HeaderText = "Broj polise",   FillWeight = 20 });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColTip",     HeaderText = "Tip",           FillWeight = 16 });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColUgovarac",HeaderText = "Ugovarač",      FillWeight = 22 });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColPremija", HeaderText = "Premija",       FillWeight = 14 });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColStatus",  HeaderText = "Status",        FillWeight = 11 });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColOd",      HeaderText = "Važi od",       FillWeight = 10 });
            dgvPolise.Columns.Add(new DataGridViewTextBoxColumn { Name = "pColDo",      HeaderText = "Važi do",       FillWeight = 10 });
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
            this.Font      = new Font("Segoe UI", 9F);
            this.Size      = new Size(1150, 720);
            this.Text      = "Polise osiguranja";
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
        private Button       btnDodaj = null!, btnIzmeni = null!, btnObrisi = null!, btnOsvezi = null!;
        private Label        lblBroj = null!, lblDetaljiNaziv = null!;
        private RichTextBox  rtbDetalji = null!;
    }
}
