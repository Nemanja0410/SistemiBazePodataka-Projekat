using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // Upravljanje predmetima osiguranja koji nisu vezani za jedan konkretan tip polise:
    // Usevi i životinje (poljoprivredno osiguranje) i pokretna imovina (imovinsko osiguranje).
    public class PredmetiOsiguranjaForma : Form
    {
        public PredmetiOsiguranjaForma()
        {
            this.Text            = "Predmeti osiguranja";
            this.Size            = new Size(760, 560);
            this.MinimumSize     = new Size(600, 420);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = UiHelper.PozadinaForm;

            var naslov = UiHelper.NapraviNaslov("🌾  Predmeti osiguranja");

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(NapraviUsevZivotinjaTab(
                "Usevi",
                () => DTOManager.vratiSveUseve(),
                dto => DTOManager.dodajUsev(dto),
                dto => DTOManager.azurirajUsev(dto),
                id  => DTOManager.obrisiUsev(id)));
            tabs.TabPages.Add(NapraviUsevZivotinjaTab(
                "Životinje",
                () => DTOManager.vratiSveZivotinje().ConvertAll(z => new UsevBasic
                    { UsevId = z.ZivotinjaId, Vrsta = z.Vrsta, Lokacija = z.Lokacija, ProcenjenaVrednost = z.ProcenjenaVrednost }),
                dto => DTOManager.dodajZivotinju(new ZivotinjaBasic { Vrsta = dto.Vrsta, Lokacija = dto.Lokacija, ProcenjenaVrednost = dto.ProcenjenaVrednost }),
                dto => DTOManager.azurirajZivotinju(new ZivotinjaBasic { ZivotinjaId = dto.UsevId, Vrsta = dto.Vrsta, Lokacija = dto.Lokacija, ProcenjenaVrednost = dto.ProcenjenaVrednost }),
                id  => DTOManager.obrisiZivotinju(id)));
            tabs.TabPages.Add(NapraviPokretnaImovinaTab());

            this.Controls.Add(tabs);
            this.Controls.Add(naslov);
        }

        // Usev i Zivotinja imaju identičnu strukturu (Vrsta/Lokacija/ProcenjenaVrednost),
        // pa oba taba dele isti UI - za Životinje se samo mapira u/iz UsevBasic radi jednostavnosti.
        private TabPage NapraviUsevZivotinjaTab(
            string naslov,
            Func<System.Collections.Generic.List<UsevBasic>> vratiSve,
            Action<UsevBasic> dodaj,
            Action<UsevBasic> azuriraj,
            Action<int> obrisi)
        {
            var tab = new TabPage(naslov) { BackColor = Color.White, Padding = new Padding(10) };

            var dgv = new DataGridView { Dock = DockStyle.Top, Height = 260 };
            UiHelper.StilizirajGrid(dgv);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVrsta", HeaderText = "Vrsta", FillWeight = 35 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLokacija", HeaderText = "Lokacija", FillWeight = 40 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVrednost", HeaderText = "Procenjena vrednost", FillWeight = 25 });

            var pnl = new TableLayoutPanel { Dock = DockStyle.Top, Height = 130, ColumnCount = 2, Padding = new Padding(0, 12, 0, 0) };
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            var txtVrsta = UiHelper.DodajRed(pnl, 0, "Vrsta *:");
            var txtLokacija = UiHelper.DodajRed(pnl, 1, "Lokacija:");
            var txtVrednost = UiHelper.DodajRed(pnl, 2, "Procenjena vrednost:");

            var pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            var btnDodaj = UiHelper.NapraviDugme("➕  Dodaj", UiHelper.Zelena, 100);
            var btnIzmeni = UiHelper.NapraviDugme("✏️  Izmeni", UiHelper.PlavaSvetla, 100);
            var btnObrisi = UiHelper.NapraviDugme("🗑️  Obriši", UiHelper.Crvena, 100);
            var btnNovo = UiHelper.NapraviDugme("🆕  Novo", UiHelper.Siva, 90);
            pnlBtn.Controls.AddRange(new Control[] { btnDodaj, btnIzmeni, btnObrisi, btnNovo });
            pnl.Controls.Add(pnlBtn, 0, 3);
            pnl.SetColumnSpan(pnlBtn, 2);

            int? izabranId = null;

            void Ucitaj()
            {
                dgv.Rows.Clear();
                foreach (var x in vratiSve())
                {
                    int r = dgv.Rows.Add(x.UsevId, x.Vrsta, x.Lokacija, x.ProcenjenaVrednost?.ToString("N2"));
                    dgv.Rows[r].Tag = x;
                }
            }

            void OcistiFormu()
            {
                izabranId = null;
                txtVrsta.Clear(); txtLokacija.Clear(); txtVrednost.Clear();
                dgv.ClearSelection();
            }

            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.CurrentRow?.Tag is not UsevBasic x) return;
                izabranId = x.UsevId;
                txtVrsta.Text = x.Vrsta ?? "";
                txtLokacija.Text = x.Lokacija ?? "";
                txtVrednost.Text = x.ProcenjenaVrednost?.ToString("F2") ?? "";
            };

            btnNovo.Click += (s, e) => OcistiFormu();

            btnDodaj.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtVrsta.Text))
                { MessageBox.Show("Vrsta je obavezna.", "Validacija"); return; }
                decimal? vrednost = null;
                if (!string.IsNullOrWhiteSpace(txtVrednost.Text) &&
                    decimal.TryParse(txtVrednost.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal v))
                    vrednost = v;

                var dto = new UsevBasic { Vrsta = txtVrsta.Text.Trim(), Lokacija = txtLokacija.Text.Trim(), ProcenjenaVrednost = vrednost };
                if (!UiHelper.PokusajAkciju(() => dodaj(dto))) return;
                OcistiFormu();
                Ucitaj();
            };

            btnIzmeni.Click += (s, e) =>
            {
                if (izabranId == null) { MessageBox.Show("Izaberite red za izmenu.", "Info"); return; }
                decimal? vrednost = null;
                if (!string.IsNullOrWhiteSpace(txtVrednost.Text) &&
                    decimal.TryParse(txtVrednost.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal v))
                    vrednost = v;

                var dto = new UsevBasic { UsevId = izabranId.Value, Vrsta = txtVrsta.Text.Trim(), Lokacija = txtLokacija.Text.Trim(), ProcenjenaVrednost = vrednost };
                if (!UiHelper.PokusajAkciju(() => azuriraj(dto))) return;
                OcistiFormu();
                Ucitaj();
            };

            btnObrisi.Click += (s, e) =>
            {
                if (izabranId == null) { MessageBox.Show("Izaberite red za brisanje.", "Info"); return; }
                if (MessageBox.Show("Obrisati izabrani zapis?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => obrisi(izabranId.Value))) return;
                OcistiFormu();
                Ucitaj();
            };

            tab.Controls.Add(pnl);
            tab.Controls.Add(dgv);
            Ucitaj();
            return tab;
        }

        private TabPage NapraviPokretnaImovinaTab()
        {
            var tab = new TabPage("Pokretna imovina") { BackColor = Color.White, Padding = new Padding(10) };

            var dgv = new DataGridView { Dock = DockStyle.Top, Height = 260 };
            UiHelper.StilizirajGrid(dgv);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNaziv", HeaderText = "Naziv", FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colOpis", HeaderText = "Opis", FillWeight = 45 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVrednost", HeaderText = "Procenjena vrednost", FillWeight = 25 });

            var pnl = new TableLayoutPanel { Dock = DockStyle.Top, Height = 130, ColumnCount = 2, Padding = new Padding(0, 12, 0, 0) };
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            var txtNaziv = UiHelper.DodajRed(pnl, 0, "Naziv *:");
            var txtOpis = UiHelper.DodajRed(pnl, 1, "Opis:");
            var txtVrednost = UiHelper.DodajRed(pnl, 2, "Procenjena vrednost:");

            var pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            var btnDodaj = UiHelper.NapraviDugme("➕  Dodaj", UiHelper.Zelena, 100);
            var btnIzmeni = UiHelper.NapraviDugme("✏️  Izmeni", UiHelper.PlavaSvetla, 100);
            var btnObrisi = UiHelper.NapraviDugme("🗑️  Obriši", UiHelper.Crvena, 100);
            var btnNovo = UiHelper.NapraviDugme("🆕  Novo", UiHelper.Siva, 90);
            pnlBtn.Controls.AddRange(new Control[] { btnDodaj, btnIzmeni, btnObrisi, btnNovo });
            pnl.Controls.Add(pnlBtn, 0, 3);
            pnl.SetColumnSpan(pnlBtn, 2);

            int? izabranId = null;

            void Ucitaj()
            {
                dgv.Rows.Clear();
                foreach (var x in DTOManager.vratiSvuPokretnuImovinu())
                {
                    int r = dgv.Rows.Add(x.PokretnaImovinaId, x.Naziv, x.Opis, x.ProcenjenaVrednost?.ToString("N2"));
                    dgv.Rows[r].Tag = x;
                }
            }

            void OcistiFormu()
            {
                izabranId = null;
                txtNaziv.Clear(); txtOpis.Clear(); txtVrednost.Clear();
                dgv.ClearSelection();
            }

            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.CurrentRow?.Tag is not PokretnaImovinaBasic x) return;
                izabranId = x.PokretnaImovinaId;
                txtNaziv.Text = x.Naziv ?? "";
                txtOpis.Text = x.Opis ?? "";
                txtVrednost.Text = x.ProcenjenaVrednost?.ToString("F2") ?? "";
            };

            btnNovo.Click += (s, e) => OcistiFormu();

            btnDodaj.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNaziv.Text))
                { MessageBox.Show("Naziv je obavezan.", "Validacija"); return; }
                decimal? vrednost = null;
                if (!string.IsNullOrWhiteSpace(txtVrednost.Text) &&
                    decimal.TryParse(txtVrednost.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal v))
                    vrednost = v;

                var dto = new PokretnaImovinaBasic { Naziv = txtNaziv.Text.Trim(), Opis = txtOpis.Text.Trim(), ProcenjenaVrednost = vrednost };
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajPokretnuImovinu(dto))) return;
                OcistiFormu();
                Ucitaj();
            };

            btnIzmeni.Click += (s, e) =>
            {
                if (izabranId == null) { MessageBox.Show("Izaberite red za izmenu.", "Info"); return; }
                decimal? vrednost = null;
                if (!string.IsNullOrWhiteSpace(txtVrednost.Text) &&
                    decimal.TryParse(txtVrednost.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal v))
                    vrednost = v;

                var dto = new PokretnaImovinaBasic { PokretnaImovinaId = izabranId.Value, Naziv = txtNaziv.Text.Trim(), Opis = txtOpis.Text.Trim(), ProcenjenaVrednost = vrednost };
                if (!UiHelper.PokusajAkciju(() => DTOManager.azurirajPokretnuImovinu(dto))) return;
                OcistiFormu();
                Ucitaj();
            };

            btnObrisi.Click += (s, e) =>
            {
                if (izabranId == null) { MessageBox.Show("Izaberite red za brisanje.", "Info"); return; }
                if (MessageBox.Show("Obrisati izabrani zapis?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiPokretnuImovinu(izabranId.Value))) return;
                OcistiFormu();
                Ucitaj();
            };

            tab.Controls.Add(pnl);
            tab.Controls.Add(dgv);
            Ucitaj();
            return tab;
        }
    }
}
