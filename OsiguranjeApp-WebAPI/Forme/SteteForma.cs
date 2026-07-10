using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class SteteForma : Form
    {
        private readonly bool _samoPregled;

        public SteteForma()
        {
            InitializeComponent();
            _samoPregled = SesijaKorisnik.ImaUlogu("LEKAR", "PRAVNIK", "PROCENITELJ");
            if (_samoPregled)
            {
                btnDodaj.Visible  = false;
                btnObrisi.Visible = false;
                UiHelper.PoravnajTraku(396, btnDodaj, btnIzmeni, btnObrisi, btnFaze, btnOsvezi, lblBroj);
            }
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
            if (_samoPregled) return;
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
            if (_samoPregled) return;
            var st = odabranaSteta();
            if (st == null) { MessageBox.Show("Izaberite štetu.", "Info"); return; }
            if (MessageBox.Show($"Obrisati štetu {st.BrojStete}?", "Potvrda",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiStetu(st.StetaId))) return;
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
}
