using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class OsobljeForma : Form
    {
        public OsobljeForma()
        {
            InitializeComponent();
            this.Load += (s, e) => this.BeginInvoke(new Action(ucitajOsoblje));
        }

        private void ucitajOsoblje()
        {
            try
            {
                var lista = DTOManager.vratiSveOsoblje(cmbTip.SelectedItem?.ToString());

                dgvOsoblje.SelectionChanged -= dgvOsoblje_SelectionChanged;
                dgvOsoblje.Rows.Clear();
                foreach (var o in lista)
                {
                    string extra = o is AgentPregled ap ? ap.RegionRada ?? "/" : "/";

                    int r = dgvOsoblje.Rows.Add(
                        o.OsobljeId,
                        $"{o.Ime} {o.Prezime}",
                        o.TipOsoblja,
                        o.Telefon,
                        o.Email,
                        extra,
                        o.Status,
                        o.DatumAngazovanja.ToString("dd.MM.yyyy"));

                    dgvOsoblje.Rows[r].Cells["oColStatus"].Style.ForeColor = UiHelper.StatusBoja(o.Status);
                    dgvOsoblje.Rows[r].Cells["oColStatus"].Style.Font =
                        new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    dgvOsoblje.Rows[r].Tag = o;
                }
                dgvOsoblje.SelectionChanged += dgvOsoblje_SelectionChanged;
                if (dgvOsoblje.Rows.Count > 0) dgvOsoblje.FirstDisplayedScrollingRowIndex = 0;
                lblBroj.Text = $"Ukupno: {lista.Count}";
                dgvOsoblje_SelectionChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška: " + ex.Message, "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private OsobljePregled? odabranoOsoblje() =>
            dgvOsoblje.CurrentRow?.Tag as OsobljePregled;

        private void cmbTip_SelectedIndexChanged(object? sender, EventArgs e) => ucitajOsoblje();
        private void btnOsvezi_Click(object? sender, EventArgs e)             => ucitajOsoblje();

        private void dgvOsoblje_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnIzmeni_Click(sender, e);
        }

        private void btnDodajAgenta_Click(object? sender, EventArgs e)
        {
            var f = new DodajAgentaForma();
            if (f.ShowDialog() == DialogResult.OK) ucitajOsoblje();
        }

        private void btnDodajOstalo_Click(object? sender, EventArgs e)
        {
            var f = new DodajOsobljeForma();
            if (f.ShowDialog() == DialogResult.OK) ucitajOsoblje();
        }

        private void btnIzmeni_Click(object? sender, EventArgs e)
        {
            var o = odabranoOsoblje();
            if (o == null) { MessageBox.Show("Izaberite zaposlenog.", "Info"); return; }
            var f = new IzmeniOsobljeForma(o);
            if (f.ShowDialog() == DialogResult.OK) ucitajOsoblje();
        }

        private void btnObrisi_Click(object? sender, EventArgs e)
        {
            var o = odabranoOsoblje();
            if (o == null) { MessageBox.Show("Izaberite zaposlenog.", "Info"); return; }
            if (MessageBox.Show(
                    $"Obrisati zaposlenog {o.Ime} {o.Prezime}?",
                    "Potvrda brisanja", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                == DialogResult.Yes)
            {
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiOsoblje(o.OsobljeId))) return;
                ucitajOsoblje();
            }
        }

        private void btnOblastiProcene_Click(object? sender, EventArgs e)
        {
            if (odabranoOsoblje() is not ProceniteljBasic pb)
            { MessageBox.Show("Izaberite procenitelja.", "Info"); return; }
            var f = new OblastiProceneForma(pb);
            f.ShowDialog();
            ucitajOsoblje();
        }

        private void dgvOsoblje_SelectionChanged(object? sender, EventArgs e)
        {
            var o = odabranoOsoblje();
            if (o == null) { btnOblastiProcene.Visible = false; return; }

            btnOblastiProcene.Visible = o is ProceniteljBasic;
            lblDetaljiNaziv.Text = $"{o.Ime} {o.Prezime}";
            rtbDetalji.Clear();
            rtbDetalji.AppendText($"Tip:            {o.TipOsoblja}\n");
            rtbDetalji.AppendText($"Status:         {o.Status}\n");
            rtbDetalji.AppendText($"Telefon:        {o.Telefon}\n");
            rtbDetalji.AppendText($"Email:          {o.Email}\n");
            rtbDetalji.AppendText($"Angažovan od:   {o.DatumAngazovanja:dd.MM.yyyy}\n");

            if (o is AgentPregled ap)
            {
                rtbDetalji.AppendText("\n── Agent detalji ──────────────────────\n");
                rtbDetalji.AppendText($"Tip agenta:     {ap.TipAgenta}\n");
                rtbDetalji.AppendText($"Licenca:        {ap.Licenca}\n");
                rtbDetalji.AppendText($"Region rada:    {ap.RegionRada}\n");
                rtbDetalji.AppendText($"Provizija:      {ap.ProvizijaProcenat:F2}%\n");
            }
            else if (o is ProceniteljBasic pb)
            {
                rtbDetalji.AppendText("\n── Procenitelj detalji ─────────────────\n");
                rtbDetalji.AppendText($"Broj licence:   {pb.BrojLicence}\n");

                rtbDetalji.AppendText("\nOblasti procene:\n");
                if (pb.Oblasti.Count == 0) rtbDetalji.AppendText("  (nema unetih oblasti)\n");
                else foreach (var ob in pb.Oblasti) rtbDetalji.AppendText($"  • {UiHelper.NazivOblasti(ob)}\n");

                rtbDetalji.AppendText($"\nProcene štete ({pb.Procene.Count}):\n");
                if (pb.Procene.Count == 0) rtbDetalji.AppendText("  (nema izvršenih procena)\n");
                else foreach (var pr in pb.Procene)
                    rtbDetalji.AppendText(
                        $"  • {pr.DatumProc:dd.MM.yyyy}  Šteta {pr.StetaBrojStete ?? $"#{pr.StetaId}"}  " +
                        $"— {pr.ProcenjeniIznos:N2} ({pr.MetodProc ?? "/"})\n");
            }
        }
    }
}
