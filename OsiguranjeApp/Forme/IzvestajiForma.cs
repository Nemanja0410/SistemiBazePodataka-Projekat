using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class IzvestajiForma : Form
    {
        public IzvestajiForma()
        {
            InitializeComponent();
            UcitajSveTabove();
        }

        private TabPage KreirajTabPolise()
        {
            var tp  = new TabPage("  📋  Polise po tipu  ");
            var dgv = NapraviGrid();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tip osiguranja",    FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Valuta",            FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Broj polisa",       FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ukupna premija",    FillWeight = 25 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Prosečna premija",  FillWeight = 25 });
            dgv.Name = "dgvPolise";

            var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White, Padding = new Padding(8, 8, 8, 0) };
            var btnExport = UiHelper.NapraviDugme("💾  Izvezi CSV", UiHelper.PlavaSvetla, 130);
            btnExport.Location = new Point(8, 8);
            btnExport.Click += (s, e) => ExportujGrid(dgv, "polise_statistika");
            pnlBtn.Controls.Add(btnExport);

            var pnlG = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            pnlG.Controls.Add(dgv);
            tp.Controls.Add(pnlG);
            tp.Controls.Add(pnlBtn);
            return tp;
        }

        private TabPage KreirajTabStete()
        {
            var tp   = new TabPage("  ⚠️  Štete  ");
            var split = new SplitContainer
            {
                Dock              = DockStyle.Fill,
                Orientation       = Orientation.Horizontal,
                SplitterDistance  = 220,
                BorderStyle       = BorderStyle.None
            };

            var lblVrsta = new Label { Text = "Po vrsti štete:", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UiHelper.Plava, Padding = new Padding(4, 2, 0, 0) };
            var dgvVrsta = NapraviGrid();
            dgvVrsta.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Vrsta štete", FillWeight = 30 });
            dgvVrsta.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Valuta",      FillWeight = 15 });
            dgvVrsta.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Broj",        FillWeight = 20 });
            dgvVrsta.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ukupno",      FillWeight = 25 });
            dgvVrsta.Name = "dgvVrsta";
            var btnExportVrsta = UiHelper.NapraviDugme("💾 CSV", UiHelper.PlavaSvetla, 80);
            btnExportVrsta.Click += (s, e) => ExportujGrid(dgvVrsta, "stete_po_vrsti");
            var pnlVrstaBtn = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = Color.White };
            pnlVrstaBtn.Controls.Add(btnExportVrsta);
            split.Panel1.Controls.Add(dgvVrsta);
            split.Panel1.Controls.Add(pnlVrstaBtn);
            split.Panel1.Controls.Add(lblVrsta);
            split.Panel1.Padding = new Padding(8, 8, 8, 4);

            var lblStat  = new Label { Text = "Po statusu:", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UiHelper.Plava, Padding = new Padding(4, 2, 0, 0) };
            var dgvStatus = NapraviGrid();
            dgvStatus.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status",     FillWeight = 25 });
            dgvStatus.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Valuta",     FillWeight = 15 });
            dgvStatus.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Broj šteta", FillWeight = 20 });
            dgvStatus.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ukupno",     FillWeight = 20 });
            dgvStatus.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Prosek",     FillWeight = 20 });
            dgvStatus.Name = "dgvStatus";
            var btnExportStatus = UiHelper.NapraviDugme("💾 CSV", UiHelper.PlavaSvetla, 80);
            btnExportStatus.Click += (s, e) => ExportujGrid(dgvStatus, "stete_statistika");
            var pnlStatusBtn = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = Color.White };
            pnlStatusBtn.Controls.Add(btnExportStatus);
            split.Panel2.Controls.Add(dgvStatus);
            split.Panel2.Controls.Add(pnlStatusBtn);
            split.Panel2.Controls.Add(lblStat);
            split.Panel2.Padding = new Padding(8, 4, 8, 8);

            tp.Controls.Add(split);
            return tp;
        }

        private TabPage KreirajTabAgenti()
        {
            var tp  = new TabPage("  👤  Agenti  ");
            var dgv = NapraviGrid();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Agent",         FillWeight = 25 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tip",           FillWeight = 12 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Region",        FillWeight = 18 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Br. polisa",    FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Provizija %",   FillWeight = 14 });
            dgv.Name = "dgvAgenti";

            var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White, Padding = new Padding(8, 8, 8, 0) };
            var btnExport = UiHelper.NapraviDugme("💾  Izvezi CSV", UiHelper.PlavaSvetla, 130);
            btnExport.Location = new Point(8, 8);
            btnExport.Click += (s, e) => ExportujGrid(dgv, "agenti_statistika");
            pnlBtn.Controls.Add(btnExport);

            var pnlG = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            pnlG.Controls.Add(dgv);
            tp.Controls.Add(pnlG);
            tp.Controls.Add(pnlBtn);
            return tp;
        }

        private TabPage KreirajTabIsticu()
        {
            var tp  = new TabPage("  ⏰  Polise koje ističu  ");
            var dgv = NapraviGrid();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Broj polise",  FillWeight = 25 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tip",          FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ugovarač",     FillWeight = 25 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Datum isteka", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Preostalo",    FillWeight = 15 });
            dgv.Name = "dgvIsticu";

            var lbl = new Label
            {
                Text = "Aktivne polise koje ističu u narednih 30 dana, sortirano po hitnosti.",
                Dock = DockStyle.Top, Height = 24, ForeColor = UiHelper.Siva, Padding = new Padding(8, 4, 0, 0)
            };

            var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White, Padding = new Padding(8, 8, 8, 0) };
            var btnExport = UiHelper.NapraviDugme("💾  Izvezi CSV", UiHelper.PlavaSvetla, 130);
            btnExport.Location = new Point(8, 8);
            btnExport.Click += (s, e) => ExportujGrid(dgv, "polise_isticu");
            pnlBtn.Controls.Add(btnExport);

            var pnlG = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            pnlG.Controls.Add(dgv);
            tp.Controls.Add(pnlG);
            tp.Controls.Add(lbl);
            tp.Controls.Add(pnlBtn);
            return tp;
        }

        private TabPage KreirajTabSirovi()
        {
            var tp = new TabPage("  💾  Polise i klijenti  ");
            var split = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                Orientation      = Orientation.Vertical,
                SplitterDistance = 500,
                BorderStyle      = BorderStyle.None
            };

            var lblP = new Label { Text = "Sve polise:", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UiHelper.Plava, Padding = new Padding(4, 2, 0, 0) };
            var dgvP = NapraviGrid();
            dgvP.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Broj polise", FillWeight = 25 });
            dgvP.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tip",         FillWeight = 20 });
            dgvP.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ugovarač",    FillWeight = 25 });
            dgvP.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Premija",     FillWeight = 15 });
            dgvP.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status",      FillWeight = 15 });
            dgvP.Name = "dgvSveP";
            var btnExportP = UiHelper.NapraviDugme("💾 Izvezi CSV", UiHelper.PlavaSvetla, 110);
            btnExportP.Click += (s, e) => ExportujGrid(dgvP, "polise");
            var pnlPBtn = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = Color.White };
            pnlPBtn.Controls.Add(btnExportP);
            split.Panel1.Controls.Add(dgvP);
            split.Panel1.Controls.Add(pnlPBtn);
            split.Panel1.Controls.Add(lblP);
            split.Panel1.Padding = new Padding(8);

            var lblK = new Label { Text = "Svi klijenti:", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UiHelper.Plava, Padding = new Padding(4, 2, 0, 0) };
            var dgvK = NapraviGrid();
            dgvK.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Naziv",   FillWeight = 30 });
            dgvK.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tip",     FillWeight = 25 });
            dgvK.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Telefon", FillWeight = 20 });
            dgvK.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status",  FillWeight = 15 });
            dgvK.Name = "dgvSviK";
            var btnExportK = UiHelper.NapraviDugme("💾 Izvezi CSV", UiHelper.PlavaSvetla, 110);
            btnExportK.Click += (s, e) => ExportujGrid(dgvK, "klijenti");
            var pnlKBtn = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = Color.White };
            pnlKBtn.Controls.Add(btnExportK);
            split.Panel2.Controls.Add(dgvK);
            split.Panel2.Controls.Add(pnlKBtn);
            split.Panel2.Controls.Add(lblK);
            split.Panel2.Padding = new Padding(8);

            tp.Controls.Add(split);
            return tp;
        }

        private void UcitajSveTabove()
        {
            UcitajPolise();
            UcitajStete();
            UcitajAgente();
            UcitajIsticu();
            UcitajSirove();
        }

        // Iznosi u razlicitim valutama se ne smeju sabirati kao da su isti broj (RSD+EUR+USD
        // nema smisla), zato se svugde grupise po (kategorija, valuta).
        private void UcitajPolise()
        {
            try
            {
                var dgv = NadjiGrid("dgvPolise");
                if (dgv == null) return;
                dgv.Rows.Clear();

                var lista = DTOManager.vratiSvePolise();
                var grupe = lista
                    .GroupBy(p => new { p.TipOsiguranja, Valuta = p.Valuta ?? "RSD" })
                    .Select(g => new
                    {
                        g.Key.TipOsiguranja, g.Key.Valuta,
                        Broj    = g.Count(),
                        Ukupno  = g.Sum(p => p.OsnovnaPremija)
                    })
                    .OrderBy(x => x.TipOsiguranja).ThenBy(x => x.Valuta)
                    .ToList();

                foreach (var gr in grupe)
                {
                    decimal prosek = gr.Broj > 0 ? gr.Ukupno / gr.Broj : 0;
                    dgv.Rows.Add(gr.TipOsiguranja, gr.Valuta, gr.Broj, $"{gr.Ukupno:N2}", $"{prosek:N2}");
                }

                foreach (var poValuti in grupe.GroupBy(g => g.Valuta))
                {
                    int svBroj = poValuti.Sum(g => g.Broj);
                    decimal svUkupno = poValuti.Sum(g => g.Ukupno);
                    int r = dgv.Rows.Add($"UKUPNO ({poValuti.Key})", "", svBroj, $"{svUkupno:N2}", "");
                    dgv.Rows[r].DefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
                    dgv.Rows[r].DefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void UcitajStete()
        {
            try
            {
                var stete    = DTOManager.vratiSveStete();
                var dgvVrsta = NadjiGrid("dgvVrsta");
                if (dgvVrsta != null)
                {
                    dgvVrsta.Rows.Clear();
                    var grupe = stete
                        .GroupBy(s => new { s.VrstaStete, Valuta = s.Valuta ?? "RSD" })
                        .Select(g => new
                        {
                            g.Key.VrstaStete, g.Key.Valuta,
                            Broj   = g.Count(),
                            Ukupno = g.Where(s => s.ProcenjeniIznos.HasValue)
                                      .Sum(s => s.ProcenjeniIznos ?? 0)
                        })
                        .OrderBy(x => x.VrstaStete).ThenBy(x => x.Valuta);
                    foreach (var gr in grupe)
                        dgvVrsta.Rows.Add(gr.VrstaStete, gr.Valuta, gr.Broj, $"{gr.Ukupno:N2}");
                }

                var dgvStatus = NadjiGrid("dgvStatus");
                if (dgvStatus != null)
                {
                    dgvStatus.Rows.Clear();
                    var grupe = stete
                        .GroupBy(s => new { s.Status, Valuta = s.Valuta ?? "RSD" })
                        .Select(g => new
                        {
                            g.Key.Status, g.Key.Valuta,
                            Broj   = g.Count(),
                            Ukupno = g.Where(s => s.ProcenjeniIznos.HasValue)
                                      .Sum(s => s.ProcenjeniIznos ?? 0),
                            Prosek = g.Where(s => s.ProcenjeniIznos.HasValue).Any()
                                     ? g.Where(s => s.ProcenjeniIznos.HasValue)
                                        .Average(s => s.ProcenjeniIznos ?? 0) : 0
                        })
                        .OrderByDescending(x => x.Broj);
                    foreach (var gr in grupe)
                    {
                        int r = dgvStatus.Rows.Add(gr.Status, gr.Valuta, gr.Broj, $"{gr.Ukupno:N2}", $"{gr.Prosek:N2}");
                        dgvStatus.Rows[r].Cells[0].Style.ForeColor = UiHelper.StatusBoja(gr.Status);
                        dgvStatus.Rows[r].Cells[0].Style.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void UcitajAgente()
        {
            try
            {
                var dgv = NadjiGrid("dgvAgenti");
                if (dgv == null) return;
                dgv.Rows.Clear();

                foreach (var a in DTOManager.vratiSveAgente())
                    dgv.Rows.Add(
                        $"{a.Ime} {a.Prezime}",
                        a.TipAgenta,
                        a.RegionRada ?? "/",
                        a.Polise.Count,
                        $"{a.ProvizijaProcenat:F2}%");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void UcitajIsticu()
        {
            try
            {
                var dgv = NadjiGrid("dgvIsticu");
                if (dgv == null) return;
                dgv.Rows.Clear();

                var danas = DateTime.Today;
                var za30dana = danas.AddDays(30);
                var isticu = DTOManager.vratiSvePolise()
                    .Where(p => p.Status == "AKTIVNA" && p.DatumIsteka.Date >= danas && p.DatumIsteka.Date <= za30dana)
                    .OrderBy(p => p.DatumIsteka)
                    .ToList();

                foreach (var p in isticu)
                {
                    int dana = (p.DatumIsteka.Date - danas).Days;
                    dgv.Rows.Add(p.BrojPolise, p.TipOsiguranja, p.UgovaracNaziv, p.DatumIsteka.ToString("dd.MM.yyyy"), $"{dana} dana");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void UcitajSirove()
        {
            try
            {
                var dgvP = NadjiGrid("dgvSveP");
                if (dgvP != null)
                {
                    dgvP.Rows.Clear();
                    foreach (var p in DTOManager.vratiSvePolise())
                        dgvP.Rows.Add(p.BrojPolise, p.TipOsiguranja, p.UgovaracNaziv, $"{p.OsnovnaPremija:N2} {p.Valuta}", p.Status);
                }

                var dgvK = NadjiGrid("dgvSviK");
                if (dgvK != null)
                {
                    dgvK.Rows.Clear();
                    foreach (var k in DTOManager.vratiSveKlijente())
                        dgvK.Rows.Add(k.Naziv, k.TipKlijenta?.Replace("_", " "), k.Telefon, k.Status);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ExportujGrid(DataGridView dgv, string naziv)
        {
            using var sfd = new SaveFileDialog
            {
                Filter   = "CSV datoteka (*.csv)|*.csv",
                FileName = $"{naziv}_{DateTime.Now:yyyyMMdd}.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var sw = new System.IO.StreamWriter(
                    sfd.FileName, false, System.Text.Encoding.UTF8);

                var zaglavlje = new List<string>();
                foreach (DataGridViewColumn col in dgv.Columns)
                    if (col.Visible) zaglavlje.Add($"\"{col.HeaderText}\"");
                sw.WriteLine(string.Join(";", zaglavlje));

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    var celije = new List<string>();
                    foreach (DataGridViewCell cell in row.Cells)
                        if (dgv.Columns[cell.ColumnIndex].Visible)
                            celije.Add($"\"{cell.Value?.ToString()?.Replace("\"", "\"\"")}\"");
                    sw.WriteLine(string.Join(";", celije));
                }

                MessageBox.Show($"Podaci su uspešno izvezeni:\n{sfd.FileName}",
                    "Izvoz uspešan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri izvozu:\n{ex.Message}",
                    "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataGridView NapraviGrid()
        {
            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgv);
            return dgv;
        }

        private DataGridView? NadjiGrid(string name)
        {
            foreach (TabPage tab in tabControl.TabPages)
            {
                var found = NadjiKontrolaRekurzivno(tab, name) as DataGridView;
                if (found != null) return found;
            }
            return null;
        }

        private Control? NadjiKontrolaRekurzivno(Control parent, string name)
        {
            if (parent.Name == name) return parent;
            foreach (Control child in parent.Controls)
            {
                var found = NadjiKontrolaRekurzivno(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
