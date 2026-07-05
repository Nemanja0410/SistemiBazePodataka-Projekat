using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class IzvestajiForma : Form
    {
        private TabControl tabControl = null!;

        public IzvestajiForma()
        {
            InitializeComponent();
            UcitajSveTabove();
        }

        private void InitializeComponent()
        {
            this.Text      = "Izveštaji i statistike";
            this.Size      = new Size(1050, 680);
            this.BackColor = UiHelper.PozadinaForm;
            this.Font      = new Font("Segoe UI", 9f);

            var naslov = UiHelper.NapraviNaslov("📊  Izveštaji i statistike");

            tabControl = new TabControl
            {
                Dock    = DockStyle.Fill,
                Font    = new Font("Segoe UI", 9.5f),
                Padding = new System.Drawing.Point(12, 6)
            };

            tabControl.TabPages.Add(KreirajTabPolise());
            tabControl.TabPages.Add(KreirajTabStete());
            tabControl.TabPages.Add(KreirajTabAgenti());

            this.Controls.Add(tabControl);
            this.Controls.Add(naslov);
        }

        // ----------------------------------------------------------------
        //  TAB 1 — Polise po tipu
        // ----------------------------------------------------------------
        private TabPage KreirajTabPolise()
        {
            var tp  = new TabPage("  📋  Polise po tipu  ");
            var dgv = NapraviGrid();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tip osiguranja",       FillWeight = 35 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Broj polisa",          FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ukupna premija (RSD)", FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Prosečna premija",     FillWeight = 25 });
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

        // ----------------------------------------------------------------
        //  TAB 2 — Štete po vrsti i statusu
        // ----------------------------------------------------------------
        private TabPage KreirajTabStete()
        {
            var tp   = new TabPage("  ⚠️  Štete  ");
            var split = new SplitContainer
            {
                Dock              = DockStyle.Fill,
                Orientation       = Orientation.Horizontal,
                SplitterDistance  = 200,
                BorderStyle       = BorderStyle.None
            };

            var dgvVrsta = NapraviGrid();
            dgvVrsta.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Vrsta štete",   FillWeight = 30 });
            dgvVrsta.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Broj",          FillWeight = 20 });
            dgvVrsta.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ukupno (RSD)",  FillWeight = 30 });
            dgvVrsta.Name = "dgvVrsta";
            split.Panel1.Controls.Add(dgvVrsta);
            split.Panel1.Padding = new Padding(8, 8, 8, 4);

            var lblStat  = new Label { Text = "Po statusu:", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UiHelper.Plava, Padding = new Padding(4, 2, 0, 0) };
            var dgvStatus = NapraviGrid();
            dgvStatus.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status",         FillWeight = 30 });
            dgvStatus.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Broj šteta",     FillWeight = 20 });
            dgvStatus.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ukupno (RSD)",   FillWeight = 30 });
            dgvStatus.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Prosek (RSD)",   FillWeight = 25 });
            dgvStatus.Name = "dgvStatus";
            split.Panel2.Controls.Add(dgvStatus);
            split.Panel2.Controls.Add(lblStat);
            split.Panel2.Padding = new Padding(8, 4, 8, 8);

            tp.Controls.Add(split);
            return tp;
        }

        // ----------------------------------------------------------------
        //  TAB 3 — Agenti
        // ----------------------------------------------------------------
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

        // ----------------------------------------------------------------
        //  Punjenje podataka
        // ----------------------------------------------------------------
        private void UcitajSveTabove()
        {
            UcitajPolise();
            UcitajStete();
            UcitajAgente();
        }

        private void UcitajPolise()
        {
            try
            {
                var dgv = NadjiGrid("dgvPolise");
                if (dgv == null) return;
                dgv.Rows.Clear();

                var lista = DTOManager.vratiSvePolise();
                var grupe = lista
                    .GroupBy(p => p.TipOsiguranja)
                    .Select(g => new
                    {
                        Tip    = g.Key,
                        Broj   = g.Count(),
                        Ukupno = g.Sum(p => p.OsnovnaPremija)
                    })
                    .OrderByDescending(x => x.Ukupno)
                    .ToList();

                decimal svukupno = 0;
                int svBroj = 0;
                foreach (var gr in grupe)
                {
                    decimal prosek = gr.Broj > 0 ? gr.Ukupno / gr.Broj : 0;
                    dgv.Rows.Add(gr.Tip, gr.Broj, $"{gr.Ukupno:N2}", $"{prosek:N2}");
                    svukupno += gr.Ukupno;
                    svBroj   += gr.Broj;
                }

                int totRed = dgv.Rows.Add("UKUPNO", svBroj, $"{svukupno:N2}", "");
                dgv.Rows[totRed].DefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
                dgv.Rows[totRed].DefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);
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
                        .GroupBy(s => s.VrstaStete)
                        .Select(g => new
                        {
                            Vrsta  = g.Key,
                            Broj   = g.Count(),
                            Ukupno = g.Where(s => s.ProcenjeniIznos.HasValue)
                                      .Sum(s => s.ProcenjeniIznos ?? 0)
                        })
                        .OrderByDescending(x => x.Ukupno);
                    foreach (var gr in grupe)
                        dgvVrsta.Rows.Add(gr.Vrsta, gr.Broj, $"{gr.Ukupno:N2}");
                }

                var dgvStatus = NadjiGrid("dgvStatus");
                if (dgvStatus != null)
                {
                    dgvStatus.Rows.Clear();
                    var grupe = stete
                        .GroupBy(s => s.Status)
                        .Select(g => new
                        {
                            Status = g.Key,
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
                        int r = dgvStatus.Rows.Add(gr.Status, gr.Broj, $"{gr.Ukupno:N2}", $"{gr.Prosek:N2}");
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

        // ----------------------------------------------------------------
        //  Export CSV
        // ----------------------------------------------------------------
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

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------
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
