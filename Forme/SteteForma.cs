using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class SteteForma : Form
    {
        public SteteForma()
        {
            InitializeComponent();
            this.Load += (s, e) => this.BeginInvoke(new Action(ucitajStete));
        }

        private void ucitajStete()
        {
            try
            {
                var lista = DTOManager.vratiSveStete(
                    cmbVrsta.SelectedItem?.ToString(),
                    cmbStatus.SelectedItem?.ToString());

                dgvStete.SelectionChanged -= dgvStete_SelectionChanged;
                dgvStete.Rows.Clear();
                foreach (var st in lista)
                {
                    int r = dgvStete.Rows.Add(
                        st.StetaId, st.BrojStete, st.VrstaStete,
                        st.PodnosilacNaziv, st.BrojPolise,
                        st.ProcenjeniIznos.HasValue
                            ? $"{st.ProcenjeniIznos.Value:N2} RSD" : "/",
                        st.Status,
                        st.DatumPrijave.ToString("dd.MM.yyyy"));

                    dgvStete.Rows[r].Tag = st;
                }
                dgvStete.SelectionChanged += dgvStete_SelectionChanged;
                if (dgvStete.Rows.Count > 0) dgvStete.FirstDisplayedScrollingRowIndex = 0;
                lblBroj.Text = $"Ukupno: {lista.Count}";
                dgvStete_SelectionChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška: " + ex.Message, "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private StetaPregled? odabranaSteta() =>
            dgvStete.CurrentRow?.Tag as StetaPregled;

        private void cmbVrsta_SelectedIndexChanged(object? sender, EventArgs e)  => ucitajStete();
        private void cmbStatus_SelectedIndexChanged(object? sender, EventArgs e) => ucitajStete();
        private void btnOsvezi_Click(object? sender, EventArgs e)                => ucitajStete();

        private void dgvStete_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnIzmeni_Click(sender, e);
        }

        private void dgvStete_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvStete.Rows.Count) return;
            if (dgvStete.Columns[e.ColumnIndex].Name != "sColStatus") return;
            if (dgvStete.Rows[e.RowIndex].Tag is not StetaPregled st) return;

            e.CellStyle!.ForeColor = UiHelper.StatusBoja(st.Status);
            e.CellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        }

        private void btnDodaj_Click(object? sender, EventArgs e)
        {
            var f = new DodajSteteForma();
            if (f.ShowDialog() == DialogResult.OK) ucitajStete();
        }

        private void btnIzmeni_Click(object? sender, EventArgs e)
        {
            var st = odabranaSteta();
            if (st == null) { MessageBox.Show("Izaberite štetu.", "Info"); return; }
            var f = new IzmeniSteteForma(DTOManager.vratiStetu(st.StetaId));
            if (f.ShowDialog() == DialogResult.OK) ucitajStete();
        }

        private void btnObrisi_Click(object? sender, EventArgs e)
        {
            var st = odabranaSteta();
            if (st == null) { MessageBox.Show("Izaberite štetu.", "Info"); return; }
            if (MessageBox.Show($"Obrisati štetu {st.BrojStete}?", "Potvrda",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DTOManager.obrisiStetu(st.StetaId);
                ucitajStete();
            }
        }

        private void btnFaze_Click(object? sender, EventArgs e)
        {
            var st = odabranaSteta();
            if (st == null) { MessageBox.Show("Izaberite štetu.", "Info"); return; }
            var f = new FazeObradeForma(DTOManager.vratiStetu(st.StetaId));
            f.ShowDialog();
        }

        private void dgvStete_SelectionChanged(object? sender, EventArgs e)
        {
            var st = odabranaSteta();
            if (st == null) return;

            lblDetaljiNaziv.Text = st.BrojStete;
            rtbDetalji.Clear();
            rtbDetalji.AppendText($"Vrsta:          {st.VrstaStete}\n");
            rtbDetalji.AppendText($"Status:         {st.Status}\n");
            rtbDetalji.AppendText($"Polisa:         {st.BrojPolise}\n");
            rtbDetalji.AppendText($"Podnosilac:     {st.PodnosilacNaziv}\n\n");
            rtbDetalji.AppendText("── Događaj ───────────────────────────\n");
            rtbDetalji.AppendText($"Datum nastanka: {st.DatumNastanka:dd.MM.yyyy}\n");
            rtbDetalji.AppendText($"Datum prijave:  {st.DatumPrijave:dd.MM.yyyy}\n");
            rtbDetalji.AppendText($"Lokacija:       {st.Lokacija}\n\n");
            rtbDetalji.AppendText("── Opis ──────────────────────────────\n");
            rtbDetalji.AppendText($"{st.OpisDogodjaja}\n");
            if (st.ProcenjeniIznos.HasValue)
                rtbDetalji.AppendText($"\nProcenjeni iznos: {st.ProcenjeniIznos.Value:N2} RSD\n");
        }
    }

    partial class SteteForma
    {
        private System.ComponentModel.IContainer? components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            var naslov = UiHelper.NapraviNaslov("⚠️  Upravljanje štetama");

            var pnlT = new Panel { BackColor = Color.White, Dock = DockStyle.Top, Height = 50 };

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
            btnOsvezi = NB("🔄  Osveži", UiHelper.Siva,        85,  ref bx);
            lblBroj   = new Label { AutoSize = true, Location = new Point(bx + 6, 16), ForeColor = UiHelper.Siva, Text = "Ukupno: 0" };

            btnDodaj.Click  += btnDodaj_Click;
            btnIzmeni.Click += btnIzmeni_Click;
            btnObrisi.Click += btnObrisi_Click;
            btnFaze.Click   += btnFaze_Click;
            btnOsvezi.Click += btnOsvezi_Click;

            pnlT.Controls.AddRange(new Control[] { cmbVrsta, cmbStatus, btnDodaj, btnIzmeni, btnObrisi, btnFaze, btnOsvezi, lblBroj });

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 660, BorderStyle = BorderStyle.None };

            dgvStete = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvStete);
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColId",      HeaderText = "ID",             Visible = false });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColBroj",    HeaderText = "Broj štete",     FillWeight = 20 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColVrsta",   HeaderText = "Vrsta",          FillWeight = 14 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColPod",     HeaderText = "Podnosilac",     FillWeight = 22 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColPolisa",  HeaderText = "Polisa",         FillWeight = 18 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColIznos",   HeaderText = "Procenjeni iznos", FillWeight = 14 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColStatus",  HeaderText = "Status",         FillWeight = 12 });
            dgvStete.Columns.Add(new DataGridViewTextBoxColumn { Name = "sColDat",     HeaderText = "Datum prijave",  FillWeight = 12 });
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
        private Button       btnDodaj = null!, btnIzmeni = null!, btnObrisi = null!, btnFaze = null!, btnOsvezi = null!;
        private Label        lblBroj = null!, lblDetaljiNaziv = null!;
        private RichTextBox  rtbDetalji = null!;
    }
}
