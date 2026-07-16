using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // Upravljanje predmetima osiguranja koji nisu vezani za jedan konkretan tip polise:
    // Usevi i životinje (poljoprivredno osiguranje) i pokretna imovina (imovinsko osiguranje).
    public partial class PredmetiOsiguranjaForma : Form
    {
        public PredmetiOsiguranjaForma()
        {
            InitializeComponent();
        }

        private static (Button btnDodaj, Button btnIzmeni, Button btnObrisi, Button btnOsvezi, Label lblBroj, Panel pnlToolbar)
            NapraviToolbar()
        {
            var pnlToolbar = new Panel
            {
                BackColor = Color.White, Dock = DockStyle.Top,
                Height = 50, Padding = new Padding(8, 10, 8, 0)
            };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            var btnDodaj  = UiHelper.NapraviDugme("➕  Dodaj",  UiHelper.Zelena,      100);
            var btnIzmeni = UiHelper.NapraviDugme("✏️  Izmeni", UiHelper.PlavaSvetla, 100);
            var btnObrisi = UiHelper.NapraviDugme("🗑️  Obriši", UiHelper.Crvena,      100);
            var btnOsvezi = UiHelper.NapraviDugme("🔄  Osveži", UiHelper.Siva,        100);
            var lblBroj   = new Label { AutoSize = true, ForeColor = UiHelper.Siva, Padding = new Padding(12, 8, 0, 0), Text = "Ukupno: 0" };
            flow.Controls.AddRange(new Control[] { btnDodaj, btnIzmeni, btnObrisi, btnOsvezi, lblBroj });
            pnlToolbar.Controls.Add(flow);
            return (btnDodaj, btnIzmeni, btnObrisi, btnOsvezi, lblBroj, pnlToolbar);
        }

        // Popup forma za unos/izmenu Useva ili Životinje (identična struktura: Vrsta/Lokacija/Procenjena vrednost).
        private static UsevBasic? PrikaziUsevZivotinjaDialog(string naslov, UsevBasic? postojeci)
        {
            using var f = new Form
            {
                Text            = naslov,
                Size            = new Size(420, 240),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                StartPosition   = FormStartPosition.CenterParent,
                Font            = new Font("Segoe UI", 9f),
                BackColor       = Color.White
            };

            var tbl = UiHelper.NapraviLayout(4);
            var txtVrsta    = UiHelper.DodajRed(tbl, 0, "Vrsta *:");
            var txtLokacija = UiHelper.DodajRed(tbl, 1, "Lokacija:");
            var txtVrednost = UiHelper.DodajRed(tbl, 2, "Procenjena vrednost:");
            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 3);

            if (postojeci != null)
            {
                txtVrsta.Text    = postojeci.Vrsta ?? "";
                txtLokacija.Text = postojeci.Lokacija ?? "";
                txtVrednost.Text = postojeci.ProcenjenaVrednost?.ToString("F2") ?? "";
            }

            UsevBasic? rezultat = null;
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtVrsta.Text))
                { MessageBox.Show("Vrsta je obavezna.", "Validacija"); txtVrsta.Focus(); return; }
                decimal? vrednost = null;
                if (!string.IsNullOrWhiteSpace(txtVrednost.Text) &&
                    decimal.TryParse(txtVrednost.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal v))
                    vrednost = v;

                rezultat = new UsevBasic
                {
                    UsevId             = postojeci?.UsevId ?? 0,
                    Vrsta              = txtVrsta.Text.Trim(),
                    Lokacija           = txtLokacija.Text.Trim(),
                    ProcenjenaVrednost = vrednost
                };
                f.DialogResult = DialogResult.OK;
                f.Close();
            };
            btnCancel.Click += (s, e) => { f.DialogResult = DialogResult.Cancel; f.Close(); };

            f.Controls.Add(tbl);
            return f.ShowDialog() == DialogResult.OK ? rezultat : null;
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

            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgv);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVrsta", HeaderText = "Vrsta", FillWeight = 35 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLokacija", HeaderText = "Lokacija", FillWeight = 40 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVrednost", HeaderText = "Procenjena vrednost", FillWeight = 25 });

            var (btnDodaj, btnIzmeni, btnObrisi, btnOsvezi, lblBroj, pnlToolbar) = NapraviToolbar();

            void Ucitaj()
            {
                dgv.Rows.Clear();
                var lista = vratiSve();
                foreach (var x in lista)
                {
                    int r = dgv.Rows.Add(x.UsevId, x.Vrsta, x.Lokacija, x.ProcenjenaVrednost?.ToString("N2"));
                    dgv.Rows[r].Tag = x;
                }
                lblBroj.Text = $"Ukupno: {lista.Count}";
            }

            UsevBasic? Izabran() => dgv.CurrentRow?.Tag as UsevBasic;

            btnDodaj.Click += (s, e) =>
            {
                var rez = PrikaziUsevZivotinjaDialog($"Dodaj - {naslov}", null);
                if (rez == null) return;
                if (!UiHelper.PokusajAkciju(() => dodaj(rez))) return;
                Ucitaj();
            };

            void IzmeniIzabrani()
            {
                var trenutni = Izabran();
                if (trenutni == null) { MessageBox.Show("Izaberite red za izmenu.", "Info"); return; }
                var rez = PrikaziUsevZivotinjaDialog($"Izmeni - {naslov}", trenutni);
                if (rez == null) return;
                rez.UsevId = trenutni.UsevId;
                if (!UiHelper.PokusajAkciju(() => azuriraj(rez))) return;
                Ucitaj();
            }
            btnIzmeni.Click += (s, e) => IzmeniIzabrani();
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) IzmeniIzabrani(); };

            btnObrisi.Click += (s, e) =>
            {
                var trenutni = Izabran();
                if (trenutni == null) { MessageBox.Show("Izaberite red za brisanje.", "Info"); return; }
                if (MessageBox.Show("Obrisati izabrani zapis?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => obrisi(trenutni.UsevId))) return;
                Ucitaj();
            };

            btnOsvezi.Click += (s, e) => Ucitaj();

            tab.Controls.Add(dgv);
            tab.Controls.Add(pnlToolbar);
            Ucitaj();
            return tab;
        }

        // Popup forma za unos/izmenu pokretne imovine (Naziv/Opis/Procenjena vrednost).
        private static PokretnaImovinaBasic? PrikaziPokretnaImovinaDialog(string naslov, PokretnaImovinaBasic? postojeci)
        {
            using var f = new Form
            {
                Text            = naslov,
                Size            = new Size(420, 240),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                StartPosition   = FormStartPosition.CenterParent,
                Font            = new Font("Segoe UI", 9f),
                BackColor       = Color.White
            };

            var tbl = UiHelper.NapraviLayout(4);
            var txtNaziv    = UiHelper.DodajRed(tbl, 0, "Naziv *:");
            var txtOpis     = UiHelper.DodajRed(tbl, 1, "Opis:");
            var txtVrednost = UiHelper.DodajRed(tbl, 2, "Procenjena vrednost:");
            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 3);

            if (postojeci != null)
            {
                txtNaziv.Text    = postojeci.Naziv ?? "";
                txtOpis.Text     = postojeci.Opis ?? "";
                txtVrednost.Text = postojeci.ProcenjenaVrednost?.ToString("F2") ?? "";
            }

            PokretnaImovinaBasic? rezultat = null;
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNaziv.Text))
                { MessageBox.Show("Naziv je obavezan.", "Validacija"); txtNaziv.Focus(); return; }
                decimal? vrednost = null;
                if (!string.IsNullOrWhiteSpace(txtVrednost.Text) &&
                    decimal.TryParse(txtVrednost.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal v))
                    vrednost = v;

                rezultat = new PokretnaImovinaBasic
                {
                    PokretnaImovinaId  = postojeci?.PokretnaImovinaId ?? 0,
                    Naziv              = txtNaziv.Text.Trim(),
                    Opis               = txtOpis.Text.Trim(),
                    ProcenjenaVrednost = vrednost
                };
                f.DialogResult = DialogResult.OK;
                f.Close();
            };
            btnCancel.Click += (s, e) => { f.DialogResult = DialogResult.Cancel; f.Close(); };

            f.Controls.Add(tbl);
            return f.ShowDialog() == DialogResult.OK ? rezultat : null;
        }

        private TabPage NapraviPokretnaImovinaTab()
        {
            var tab = new TabPage("Pokretna imovina") { BackColor = Color.White, Padding = new Padding(10) };

            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgv);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNaziv", HeaderText = "Naziv", FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colOpis", HeaderText = "Opis", FillWeight = 45 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVrednost", HeaderText = "Procenjena vrednost", FillWeight = 25 });

            var (btnDodaj, btnIzmeni, btnObrisi, btnOsvezi, lblBroj, pnlToolbar) = NapraviToolbar();

            void Ucitaj()
            {
                dgv.Rows.Clear();
                var lista = DTOManager.vratiSvuPokretnuImovinu();
                foreach (var x in lista)
                {
                    int r = dgv.Rows.Add(x.PokretnaImovinaId, x.Naziv, x.Opis, x.ProcenjenaVrednost?.ToString("N2"));
                    dgv.Rows[r].Tag = x;
                }
                lblBroj.Text = $"Ukupno: {lista.Count}";
            }

            PokretnaImovinaBasic? Izabran() => dgv.CurrentRow?.Tag as PokretnaImovinaBasic;

            btnDodaj.Click += (s, e) =>
            {
                var rez = PrikaziPokretnaImovinaDialog("Dodaj - Pokretna imovina", null);
                if (rez == null) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajPokretnuImovinu(rez))) return;
                Ucitaj();
            };

            void IzmeniIzabrani()
            {
                var trenutni = Izabran();
                if (trenutni == null) { MessageBox.Show("Izaberite red za izmenu.", "Info"); return; }
                var rez = PrikaziPokretnaImovinaDialog("Izmeni - Pokretna imovina", trenutni);
                if (rez == null) return;
                rez.PokretnaImovinaId = trenutni.PokretnaImovinaId;
                if (!UiHelper.PokusajAkciju(() => DTOManager.azurirajPokretnuImovinu(rez))) return;
                Ucitaj();
            }
            btnIzmeni.Click += (s, e) => IzmeniIzabrani();
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) IzmeniIzabrani(); };

            btnObrisi.Click += (s, e) =>
            {
                var trenutni = Izabran();
                if (trenutni == null) { MessageBox.Show("Izaberite red za brisanje.", "Info"); return; }
                if (MessageBox.Show("Obrisati izabrani zapis?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiPokretnuImovinu(trenutni.PokretnaImovinaId))) return;
                Ucitaj();
            };

            btnOsvezi.Click += (s, e) => Ucitaj();

            tab.Controls.Add(dgv);
            tab.Controls.Add(pnlToolbar);
            Ucitaj();
            return tab;
        }

        // Popup forma za unos/izmenu vozila (Registracija/Marka/Model/Godina/Vlasnik).
        private static VoziloBasic? PrikaziVoziloDialog(string naslov, VoziloBasic? postojeci)
        {
            using var f = new Form
            {
                Text            = naslov,
                Size            = new Size(420, 300),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                StartPosition   = FormStartPosition.CenterParent,
                Font            = new Font("Segoe UI", 9f),
                BackColor       = Color.White
            };

            var tbl = UiHelper.NapraviLayout(6);
            var txtRegistracija = UiHelper.DodajRed(tbl, 0, "Registracija *:");
            var txtMarka        = UiHelper.DodajRed(tbl, 1, "Marka *:");
            var txtModel        = UiHelper.DodajRed(tbl, 2, "Model *:");
            var txtGodina       = UiHelper.DodajRed(tbl, 3, "Godina proizvodnje *:");
            var cmbVlasnik      = UiHelper.DodajComboRed(tbl, 4, "Vlasnik *:");
            foreach (var k in DTOManager.vratiSveKlijente())
                cmbVlasnik.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 5);

            if (postojeci != null)
            {
                txtRegistracija.Text = postojeci.Registracija ?? "";
                txtMarka.Text        = postojeci.Marka ?? "";
                txtModel.Text        = postojeci.Model ?? "";
                txtGodina.Text       = postojeci.GodinaProizvodnje.ToString();
                foreach (ComboItem it in cmbVlasnik.Items)
                    if (it.Id == postojeci.VlasnikId) { cmbVlasnik.SelectedItem = it; break; }
            }

            VoziloBasic? rezultat = null;
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtRegistracija.Text))
                { MessageBox.Show("Registracija je obavezna.", "Validacija"); txtRegistracija.Focus(); return; }
                if (string.IsNullOrWhiteSpace(txtMarka.Text))
                { MessageBox.Show("Marka je obavezna.", "Validacija"); txtMarka.Focus(); return; }
                if (string.IsNullOrWhiteSpace(txtModel.Text))
                { MessageBox.Show("Model je obavezan.", "Validacija"); txtModel.Focus(); return; }
                if (!int.TryParse(txtGodina.Text, out int godina))
                { MessageBox.Show("Godina proizvodnje mora biti broj.", "Validacija"); txtGodina.Focus(); return; }
                if (cmbVlasnik.SelectedItem is not ComboItem vlasnik)
                { MessageBox.Show("Izaberite vlasnika.", "Validacija"); cmbVlasnik.Focus(); return; }

                rezultat = new VoziloBasic
                {
                    VoziloId          = postojeci?.VoziloId ?? 0,
                    Registracija      = txtRegistracija.Text.Trim(),
                    Marka             = txtMarka.Text.Trim(),
                    Model             = txtModel.Text.Trim(),
                    GodinaProizvodnje = godina,
                    VlasnikId         = vlasnik.Id
                };
                f.DialogResult = DialogResult.OK;
                f.Close();
            };
            btnCancel.Click += (s, e) => { f.DialogResult = DialogResult.Cancel; f.Close(); };

            f.Controls.Add(tbl);
            return f.ShowDialog() == DialogResult.OK ? rezultat : null;
        }

        private TabPage NapraviVozilaTab()
        {
            var tab = new TabPage("Vozila") { BackColor = Color.White, Padding = new Padding(10) };

            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgv);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReg", HeaderText = "Registracija", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMarka", HeaderText = "Marka", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colModel", HeaderText = "Model", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colGodina", HeaderText = "Godina", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVlasnik", HeaderText = "Vlasnik", FillWeight = 25 });

            var (btnDodaj, btnIzmeni, btnObrisi, btnOsvezi, lblBroj, pnlToolbar) = NapraviToolbar();

            void Ucitaj()
            {
                dgv.Rows.Clear();
                var lista = DTOManager.vratiSvaVozila();
                foreach (var v in lista)
                {
                    int r = dgv.Rows.Add(v.VoziloId, v.Registracija, v.Marka, v.Model, v.GodinaProizvodnje, v.VlasnikNaziv);
                    dgv.Rows[r].Tag = v;
                }
                lblBroj.Text = $"Ukupno: {lista.Count}";
            }

            VoziloBasic? Izabran() => dgv.CurrentRow?.Tag as VoziloBasic;

            btnDodaj.Click += (s, e) =>
            {
                var rez = PrikaziVoziloDialog("Dodaj - Vozilo", null);
                if (rez == null) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajVozilo(rez))) return;
                Ucitaj();
            };

            void IzmeniIzabrani()
            {
                var trenutni = Izabran();
                if (trenutni == null) { MessageBox.Show("Izaberite red za izmenu.", "Info"); return; }
                var rez = PrikaziVoziloDialog("Izmeni - Vozilo", trenutni);
                if (rez == null) return;
                rez.VoziloId = trenutni.VoziloId;
                if (!UiHelper.PokusajAkciju(() => DTOManager.azurirajVozilo(rez))) return;
                Ucitaj();
            }
            btnIzmeni.Click += (s, e) => IzmeniIzabrani();
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) IzmeniIzabrani(); };

            btnObrisi.Click += (s, e) =>
            {
                var trenutni = Izabran();
                if (trenutni == null) { MessageBox.Show("Izaberite red za brisanje.", "Info"); return; }
                if (MessageBox.Show("Obrisati izabrani zapis?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiVozilo(trenutni.VoziloId))) return;
                Ucitaj();
            };

            btnOsvezi.Click += (s, e) => Ucitaj();

            tab.Controls.Add(dgv);
            tab.Controls.Add(pnlToolbar);
            Ucitaj();
            return tab;
        }

        // Popup forma za unos/izmenu nekretnine (Adresa/TipObjekta/Povrsina/GodinaIzgradnje/ProcenjenaVrednost).
        // Numericka polja su OBAVEZNA - bez toga zapis nema smisla (ista provera kao na web klijentu).
        private static NekretninaBasic? PrikaziNekretninaDialog(string naslov, NekretninaBasic? postojeci)
        {
            using var f = new Form
            {
                Text            = naslov,
                Size            = new Size(420, 340),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                StartPosition   = FormStartPosition.CenterParent,
                Font            = new Font("Segoe UI", 9f),
                BackColor       = Color.White
            };

            var tbl = UiHelper.NapraviLayout(6);
            var txtAdresa    = UiHelper.DodajRed(tbl, 0, "Adresa *:");
            var cmbTip       = UiHelper.DodajComboRed(tbl, 1, "Tip objekta *:");
            cmbTip.Items.AddRange(new[] { "STAN", "KUCA", "POSLOVNI_PROSTOR", "MAGACIN", "ZEMLJISTE", "OSTALO" });
            var txtPovrsina  = UiHelper.DodajRed(tbl, 2, "Površina (m²) *:");
            var txtGodina    = UiHelper.DodajRed(tbl, 3, "Godina izgradnje *:");
            var txtVrednost  = UiHelper.DodajRed(tbl, 4, "Procenjena vrednost *:");
            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 5);

            if (postojeci != null)
            {
                txtAdresa.Text     = postojeci.Adresa ?? "";
                cmbTip.SelectedItem = postojeci.TipObjekta;
                txtPovrsina.Text   = postojeci.Povrsina.ToString("F2");
                txtGodina.Text     = postojeci.GodinaIzgradnje.ToString();
                txtVrednost.Text   = postojeci.ProcenjenaVrednost.ToString("F2");
            }
            else
            {
                cmbTip.SelectedIndex = 0;
            }

            NekretninaBasic? rezultat = null;
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtAdresa.Text))
                { MessageBox.Show("Adresa je obavezna.", "Validacija"); txtAdresa.Focus(); return; }
                if (cmbTip.SelectedItem == null)
                { MessageBox.Show("Tip objekta je obavezan.", "Validacija"); cmbTip.Focus(); return; }
                if (!decimal.TryParse(txtPovrsina.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal povrsina) || povrsina <= 0)
                { MessageBox.Show("Površina mora biti pozitivan broj.", "Validacija"); txtPovrsina.Focus(); return; }
                if (!int.TryParse(txtGodina.Text, out int godina) || godina <= 0)
                { MessageBox.Show("Godina izgradnje mora biti pozitivan broj.", "Validacija"); txtGodina.Focus(); return; }
                if (!decimal.TryParse(txtVrednost.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal vrednost) || vrednost <= 0)
                { MessageBox.Show("Procenjena vrednost mora biti pozitivan broj.", "Validacija"); txtVrednost.Focus(); return; }

                rezultat = new NekretninaBasic
                {
                    NekretninaId       = postojeci?.NekretninaId ?? 0,
                    Adresa             = txtAdresa.Text.Trim(),
                    TipObjekta         = cmbTip.SelectedItem.ToString(),
                    Povrsina           = povrsina,
                    GodinaIzgradnje    = godina,
                    ProcenjenaVrednost = vrednost
                };
                f.DialogResult = DialogResult.OK;
                f.Close();
            };
            btnCancel.Click += (s, e) => { f.DialogResult = DialogResult.Cancel; f.Close(); };

            f.Controls.Add(tbl);
            return f.ShowDialog() == DialogResult.OK ? rezultat : null;
        }

        private TabPage NapraviNekretnineTab()
        {
            var tab = new TabPage("Nekretnine") { BackColor = Color.White, Padding = new Padding(10) };

            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgv);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAdresa", HeaderText = "Adresa", FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTip", HeaderText = "Tip objekta", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPovrsina", HeaderText = "Površina", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colGodina", HeaderText = "Godina", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVrednost", HeaderText = "Procenjena vrednost", FillWeight = 20 });

            var (btnDodaj, btnIzmeni, btnObrisi, btnOsvezi, lblBroj, pnlToolbar) = NapraviToolbar();

            void Ucitaj()
            {
                dgv.Rows.Clear();
                var lista = DTOManager.vratiSveNekretnine();
                foreach (var n in lista)
                {
                    int r = dgv.Rows.Add(n.NekretninaId, n.Adresa, n.TipObjekta, n.Povrsina.ToString("N2"), n.GodinaIzgradnje, n.ProcenjenaVrednost.ToString("N2"));
                    dgv.Rows[r].Tag = n;
                }
                lblBroj.Text = $"Ukupno: {lista.Count}";
            }

            NekretninaBasic? Izabran() => dgv.CurrentRow?.Tag as NekretninaBasic;

            btnDodaj.Click += (s, e) =>
            {
                var rez = PrikaziNekretninaDialog("Dodaj - Nekretnina", null);
                if (rez == null) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajNekretninu(rez))) return;
                Ucitaj();
            };

            void IzmeniIzabrani()
            {
                var trenutni = Izabran();
                if (trenutni == null) { MessageBox.Show("Izaberite red za izmenu.", "Info"); return; }
                var rez = PrikaziNekretninaDialog("Izmeni - Nekretnina", trenutni);
                if (rez == null) return;
                rez.NekretninaId = trenutni.NekretninaId;
                if (!UiHelper.PokusajAkciju(() => DTOManager.azurirajNekretninu(rez))) return;
                Ucitaj();
            }
            btnIzmeni.Click += (s, e) => IzmeniIzabrani();
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) IzmeniIzabrani(); };

            btnObrisi.Click += (s, e) =>
            {
                var trenutni = Izabran();
                if (trenutni == null) { MessageBox.Show("Izaberite red za brisanje.", "Info"); return; }
                if (MessageBox.Show("Obrisati izabrani zapis?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiNekretninu(trenutni.NekretninaId))) return;
                Ucitaj();
            };

            btnOsvezi.Click += (s, e) => Ucitaj();

            tab.Controls.Add(dgv);
            tab.Controls.Add(pnlToolbar);
            Ucitaj();
            return tab;
        }
    }
}
