using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // Nova šteta - AUTO/ZDRAVSTVENA/IMOVINSKA imaju sopstvena polja, plus Oštećena lica/Osteceni
    // predmeti/Procene štete direktno u istoj formi (pending liste do dobijanja StetaId,
    // isto nacelo kao DodajPolisaForma). Faze obrade i Fotografije ostaju posebni dijalozi.
    public partial class DodajSteteForma : Form
    {
        private List<PolisaPregled> _svePolise = new();
        private List<KlijentPregled> _klijenti = new();
        private readonly bool _smeProcena = SesijaKorisnik.ImaUlogu("ADMIN", "PROCENITELJ");

        private readonly List<OstecenLiceBasic> _pendingLica = new();
        private readonly List<OsteceniPredmetBasic> _pendingPredmeti = new();
        private readonly List<ProcenaStetaBasic> _pendingProcene = new();

        public DodajSteteForma()
        {
            UcitajComboove();
            InitializeComponent();
            AzurirajTipSpecificnaPolja();
        }

        private void UcitajComboove()
        {
            _svePolise = DTOManager.vratiSvePolise();
            _klijenti = DTOManager.vratiSveKlijente();
        }

        private GroupBox NapraviBaznaPoljaGrupa()
        {
            var grp = new GroupBox { Text = "Osnovni podaci", Width = 500, Height = 320 };
            var tbl = UiHelper.NapraviLayout(9);
            tbl.Dock = DockStyle.Fill;

            cmbVrsta = UiHelper.DodajComboRed(tbl, 0, "Vrsta štete *:");
            cmbVrsta.Items.AddRange(new object[] { "AUTO", "ZDRAVSTVENA", "IMOVINSKA", "PUTNA", "ZIVOTNA", "OSTALO" });
            cmbVrsta.SelectedIndex = 0;
            cmbVrsta.SelectedIndexChanged += (s, e) => { txtBroj.Text = GenerisiBroj(); AzurirajTipSpecificnaPolja(); };

            txtBroj = UiHelper.DodajRed(tbl, 1, "Broj štete *:");
            cmbPodnosilac = UiHelper.DodajComboRed(tbl, 2, "Podnosilac *:");
            cmbPodnosilac.Items.Add(new ComboItem(0, "-- Izaberi klijenta --"));
            foreach (var k in _klijenti) cmbPodnosilac.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbPodnosilac.SelectedIndex = 0;
            cmbPodnosilac.SelectedIndexChanged += (s, e) => OsveziPolise((cmbPodnosilac.SelectedItem as ComboItem)?.Id ?? 0);

            cmbPolisa = UiHelper.DodajComboRed(tbl, 3, "Polisa *:");
            dtpNastanka = UiHelper.DodajDTPRed(tbl, 4, "Datum nastanka *:");
            txtLokacija = UiHelper.DodajRed(tbl, 5, "Lokacija:");
            txtOpis = UiHelper.DodajRed(tbl, 6, "Opis događaja:");
            txtIznos = UiHelper.DodajRed(tbl, 7, "Procenjeni iznos:");

            var tblDruga = UiHelper.NapraviLayout(2);
            tblDruga.Location = new Point(0, 320); tblDruga.Height = 40;
            cmbValuta = UiHelper.DodajComboRed(tblDruga, 0, "Valuta:");
            cmbValuta.Items.AddRange(new object[] { "RSD", "EUR", "USD" });
            cmbValuta.SelectedIndex = 0;
            cmbStatus = UiHelper.DodajComboRed(tblDruga, 1, "Status:");
            cmbStatus.Items.AddRange(new object[] { "PRIJAVLJENA", "U_OBRADI", "U_PROCENI", "ODOBRENA", "ODBIJENA", "ISPLACENA", "ZATVORENA" });
            cmbStatus.SelectedIndex = 0;

            grp.Controls.Add(tbl);
            grp.Controls.Add(tblDruga);
            grp.Height = 360;

            OsveziPolise(0);
            txtBroj.Text = GenerisiBroj();
            return grp;
        }

        private void OsveziPolise(int klijentId)
        {
            cmbPolisa.Items.Clear();
            cmbPolisa.Items.Add(new ComboItem(0, "-- Izaberi polisu --"));
            string tip = cmbVrsta?.SelectedItem?.ToString() ?? "";
            var dozvoljeni = tip switch
            {
                "AUTO" => new[] { "AUTO" }, "ZDRAVSTVENA" => new[] { "ZDRAVSTVENO" },
                "IMOVINSKA" => new[] { "IMOVINSKO" }, "PUTNA" => new[] { "PUTNO" },
                "ZIVOTNA" => new[] { "ZIVOTNO" },
                _ => new[] { "POLJOPRIVREDNO", "ODGOVORNOST", "SPECIJALIZOVANO" }
            };
            foreach (var p in _svePolise.Where(p => p.UgovaracId == klijentId && dozvoljeni.Contains(p.TipOsiguranja)))
                cmbPolisa.Items.Add(new ComboItem(p.PolisaId, $"{p.BrojPolise} ({p.TipOsiguranja})"));
            cmbPolisa.SelectedIndex = 0;
            cmbPolisa.Enabled = klijentId != 0;
        }

        private string GenerisiBroj()
        {
            var v = cmbVrsta.SelectedItem?.ToString() ?? "ST";
            var pref = v.Length >= 3 ? v.Substring(0, 3) : v;
            return $"STE-{pref}-{DateTime.Now.Year}-{new Random().Next(100, 999)}";
        }

        private void AzurirajTipSpecificnaPolja()
        {
            pnlTipSpecificno.Controls.Clear();
            cmbVozilo = null; txtZapisnik = txtServis = null;
            txtDijagnoza = txtMedDok = txtZdravUstanova = null; cmbLekar = null;
            txtProcenaOstecenja = txtIzvodjacSanacije = null;
            OsveziPolise((cmbPodnosilac?.SelectedItem as ComboItem)?.Id ?? 0);

            string tip = cmbVrsta.SelectedItem?.ToString() ?? "";
            if (tip is not ("AUTO" or "ZDRAVSTVENA" or "IMOVINSKA")) { pnlTipSpecificno.Height = 0; return; }

            var grp = new GroupBox { Text = $"{NazivVrste(tip)} — dodatni podaci", Width = 500 };
            TableLayoutPanel tbl;

            switch (tip)
            {
                case "AUTO":
                    tbl = UiHelper.NapraviLayout(3);
                    cmbVozilo = UiHelper.DodajComboRed(tbl, 0, "Vozilo:");
                    cmbVozilo.Items.Add(new ComboItem(0, "-- nije poznato --"));
                    foreach (var v in DTOManager.vratiSvaVozila()) cmbVozilo.Items.Add(new ComboItem(v.VoziloId, $"{v.Registracija} — {v.Marka} {v.Model}"));
                    cmbVozilo.SelectedIndex = 0;
                    txtZapisnik = UiHelper.DodajRed(tbl, 1, "Zapisnik policije:");
                    txtServis = UiHelper.DodajRed(tbl, 2, "Servis:");
                    grp.Controls.Add(tbl);
                    grp.Height = 150;
                    break;

                case "ZDRAVSTVENA":
                    tbl = UiHelper.NapraviLayout(4);
                    txtDijagnoza = UiHelper.DodajRed(tbl, 0, "Dijagnoza:");
                    txtMedDok = UiHelper.DodajRed(tbl, 1, "Medicinska dokumentacija:");
                    txtZdravUstanova = UiHelper.DodajRed(tbl, 2, "Zdravstvena ustanova:");
                    cmbLekar = UiHelper.DodajComboRed(tbl, 3, "Lekar konsultant:");
                    cmbLekar.Items.Add(new ComboItem(0, "-- nije poznato --"));
                    foreach (var o in DTOManager.vratiSveOsoblje("LEKAR")) cmbLekar.Items.Add(new ComboItem(o.OsobljeId, $"{o.Ime} {o.Prezime}"));
                    cmbLekar.SelectedIndex = 0;
                    grp.Controls.Add(tbl);
                    grp.Height = 190;
                    break;

                case "IMOVINSKA":
                    tbl = UiHelper.NapraviLayout(2);
                    txtProcenaOstecenja = UiHelper.DodajRed(tbl, 0, "Procena oštećenja:");
                    txtIzvodjacSanacije = UiHelper.DodajRed(tbl, 1, "Izvođač sanacije:");
                    grp.Controls.Add(tbl);
                    grp.Height = 110;
                    break;
            }

            pnlTipSpecificno.Controls.Add(grp);
            pnlTipSpecificno.Height = grp.Height;
        }

        private static string NazivVrste(string v) => v switch
        {
            "AUTO" => "Auto šteta", "ZDRAVSTVENA" => "Zdravstvena šteta", "IMOVINSKA" => "Imovinska šteta", _ => v
        };

        // ---------- Ostecena lica (fold-in) ----------

        private GroupBox NapraviOstecenaLicaGrupa()
        {
            var grp = new GroupBox { Text = "Oštećena lica", Width = 500, Height = 150 };
            lstPendingLica = new ListBox { Location = new Point(8, 20), Size = new Size(484, 55) };
            grp.Controls.Add(lstPendingLica);

            cmbOlKlijent = new ComboBox { Location = new Point(8, 82), Size = new Size(140, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbOlKlijent.Items.Add(new ComboItem(0, "-- nije klijent --"));
            foreach (var k in _klijenti) cmbOlKlijent.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbOlKlijent.SelectedIndex = 0;
            txtOlImePrezime = new TextBox { Location = new Point(152, 82), Size = new Size(130, 24), PlaceholderText = "ili ime i prezime" };
            txtOlOpis = new TextBox { Location = new Point(286, 82), Size = new Size(120, 24), PlaceholderText = "Opis povrede" };
            txtOlIznos = new TextBox { Location = new Point(410, 82), Size = new Size(50, 24), PlaceholderText = "Iznos" };
            var btnDodaj = UiHelper.NapraviDugme("➕", UiHelper.Zelena, 30);
            btnDodaj.Location = new Point(410, 110);
            var btnUkloni = UiHelper.NapraviDugme("🗑️", UiHelper.Crvena, 30);
            btnUkloni.Location = new Point(452, 110);

            btnDodaj.Click += (s, e) =>
            {
                var izabran = cmbOlKlijent.SelectedItem as ComboItem;
                string ime = txtOlImePrezime.Text.Trim();
                if ((izabran == null || izabran.Id == 0) && string.IsNullOrEmpty(ime))
                { MessageBox.Show("Izaberite klijenta ili unesite ime i prezime.", "Validacija"); return; }
                decimal? ParsiOpc(string t) => decimal.TryParse(t.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : (decimal?)null;
                var dto = new OstecenLiceBasic
                {
                    KlijentId = izabran?.Id > 0 ? izabran.Id : null,
                    KlijentNaziv = izabran?.Id > 0 ? izabran.Tekst : null,
                    ImePrezime = izabran?.Id > 0 ? null : ime,
                    OpisPovrede = txtOlOpis.Text.Trim(), IznosNaknade = ParsiOpc(txtOlIznos.Text)
                };
                _pendingLica.Add(dto);
                lstPendingLica.Items.Add($"{dto}: {dto.OpisPovrede}");
                cmbOlKlijent.SelectedIndex = 0; txtOlImePrezime.Clear(); txtOlOpis.Clear(); txtOlIznos.Clear();
            };
            btnUkloni.Click += (s, e) =>
            {
                int idx = lstPendingLica.SelectedIndex;
                if (idx < 0) return;
                _pendingLica.RemoveAt(idx); lstPendingLica.Items.RemoveAt(idx);
            };

            grp.Controls.AddRange(new Control[] { cmbOlKlijent, txtOlImePrezime, txtOlOpis, txtOlIznos, btnDodaj, btnUkloni });
            return grp;
        }

        // ---------- Osteceni predmeti (fold-in) ----------

        private GroupBox NapraviOsteceniPredmetiGrupa()
        {
            var grp = new GroupBox { Text = "Oštećeni predmeti", Width = 500, Height = 150 };
            lstPendingPredmeti = new ListBox { Location = new Point(8, 20), Size = new Size(484, 55) };
            grp.Controls.Add(lstPendingPredmeti);

            txtOpTip = new TextBox { Location = new Point(8, 82), Size = new Size(150, 24), PlaceholderText = "Tip predmeta" };
            txtOpOpis = new TextBox { Location = new Point(164, 82), Size = new Size(200, 24), PlaceholderText = "Opis oštećenja" };
            txtOpIznos = new TextBox { Location = new Point(370, 82), Size = new Size(70, 24), PlaceholderText = "Iznos" };
            var btnDodaj = UiHelper.NapraviDugme("➕", UiHelper.Zelena, 30);
            btnDodaj.Location = new Point(410, 110);
            var btnUkloni = UiHelper.NapraviDugme("🗑️", UiHelper.Crvena, 30);
            btnUkloni.Location = new Point(452, 110);

            btnDodaj.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtOpTip.Text))
                { MessageBox.Show("Tip predmeta je obavezan.", "Validacija"); return; }
                decimal? ParsiOpc(string t) => decimal.TryParse(t.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : (decimal?)null;
                var dto = new OsteceniPredmetBasic { TipPredmeta = txtOpTip.Text.Trim(), OpisOstecenja = txtOpOpis.Text.Trim(), ProcenjeniIznos = ParsiOpc(txtOpIznos.Text) };
                _pendingPredmeti.Add(dto);
                lstPendingPredmeti.Items.Add($"{dto.TipPredmeta}: {dto.OpisOstecenja}");
                txtOpTip.Clear(); txtOpOpis.Clear(); txtOpIznos.Clear();
            };
            btnUkloni.Click += (s, e) =>
            {
                int idx = lstPendingPredmeti.SelectedIndex;
                if (idx < 0) return;
                _pendingPredmeti.RemoveAt(idx); lstPendingPredmeti.Items.RemoveAt(idx);
            };

            grp.Controls.AddRange(new Control[] { txtOpTip, txtOpOpis, txtOpIznos, btnDodaj, btnUkloni });
            return grp;
        }

        // ---------- Procene stete (fold-in, samo ADMIN/PROCENITELJ) ----------

        private GroupBox NapraviProceneGrupa()
        {
            var grp = new GroupBox { Text = "Procene štete", Width = 500, Height = 180 };
            lstPendingProcene = new ListBox { Location = new Point(8, 20), Size = new Size(484, 55) };
            grp.Controls.Add(lstPendingProcene);

            cmbPrProcenitelj = new ComboBox { Location = new Point(8, 82), Size = new Size(220, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPrProcenitelj.Items.Add(new ComboItem(0, "-- izaberite procenitelja --"));
            foreach (var o in DTOManager.vratiSveOsoblje("PROCENITELJ")) cmbPrProcenitelj.Items.Add(new ComboItem(o.OsobljeId, $"{o.Ime} {o.Prezime}"));
            cmbPrProcenitelj.SelectedIndex = 0;
            txtPrMetod = new TextBox { Location = new Point(234, 82), Size = new Size(140, 24), PlaceholderText = "Metod procene" };
            txtPrIznos = new TextBox { Location = new Point(380, 82), Size = new Size(80, 24), PlaceholderText = "Iznos" };
            txtPrNalaz = new TextBox { Location = new Point(8, 110), Size = new Size(260, 24), PlaceholderText = "Nalaz" };
            txtPrPreporuka = new TextBox { Location = new Point(274, 110), Size = new Size(180, 24), PlaceholderText = "Preporuka" };
            var btnDodaj = UiHelper.NapraviDugme("➕", UiHelper.Zelena, 30);
            btnDodaj.Location = new Point(460, 110);
            var btnUkloni = UiHelper.NapraviDugme("🗑️", UiHelper.Crvena, 30);
            btnUkloni.Location = new Point(410, 138);

            btnDodaj.Click += (s, e) =>
            {
                if ((cmbPrProcenitelj.SelectedItem as ComboItem)?.Id is not int procId || procId == 0)
                { MessageBox.Show("Izaberite procenitelja.", "Validacija"); return; }
                if (!decimal.TryParse(txtPrIznos.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal iznos))
                { MessageBox.Show("Iznos mora biti broj.", "Validacija"); return; }
                var dto = new ProcenaStetaBasic
                {
                    ProceniteljId = procId, DatumProc = DateTime.Today,
                    MetodProc = txtPrMetod.Text.Trim(), Nalaz = txtPrNalaz.Text.Trim(),
                    ProcenjeniIznos = iznos, Preporuka = txtPrPreporuka.Text.Trim()
                };
                _pendingProcene.Add(dto);
                lstPendingProcene.Items.Add($"{(cmbPrProcenitelj.SelectedItem as ComboItem)?.Tekst}: {dto.ProcenjeniIznos:N2}");
                cmbPrProcenitelj.SelectedIndex = 0; txtPrMetod.Clear(); txtPrNalaz.Clear(); txtPrIznos.Clear(); txtPrPreporuka.Clear();
            };
            btnUkloni.Click += (s, e) =>
            {
                int idx = lstPendingProcene.SelectedIndex;
                if (idx < 0) return;
                _pendingProcene.RemoveAt(idx); lstPendingProcene.Items.RemoveAt(idx);
            };

            grp.Controls.AddRange(new Control[] { cmbPrProcenitelj, txtPrMetod, txtPrIznos, txtPrNalaz, txtPrPreporuka, btnDodaj, btnUkloni });
            return grp;
        }

        // ---------- Cuvanje ----------

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if ((cmbPolisa.SelectedItem as ComboItem)?.Id == 0)
            { MessageBox.Show("Izaberite polisu.", "Validacija"); return; }
            if ((cmbPodnosilac.SelectedItem as ComboItem)?.Id == 0)
            { MessageBox.Show("Izaberite podnosioca.", "Validacija"); return; }

            decimal? iznos = null;
            if (!string.IsNullOrWhiteSpace(txtIznos.Text) &&
                decimal.TryParse(txtIznos.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal iz))
                iznos = iz;

            string vrsta = cmbVrsta.SelectedItem?.ToString() ?? "";
            var baza = new StetaBasic
            {
                BrojStete = txtBroj.Text.Trim(), VrstaStete = vrsta,
                PolisaId = ((ComboItem)cmbPolisa.SelectedItem!).Id,
                PodnosilacId = ((ComboItem)cmbPodnosilac.SelectedItem!).Id,
                DatumNastanka = dtpNastanka.Value.Date,
                Lokacija = txtLokacija.Text.Trim(), OpisDogodjaja = txtOpis.Text.Trim(),
                ProcenjeniIznos = iznos, Valuta = cmbValuta.SelectedItem?.ToString(),
                Status = cmbStatus.SelectedItem?.ToString()
            };

            int noviId = vrsta switch
            {
                "AUTO" => DTOManager.dodajAutoStetu(KopirajU<AutoStetaBasic>(baza, a =>
                {
                    a.ZapisnikPolicije = txtZapisnik!.Text.Trim(); a.Servis = txtServis!.Text.Trim();
                    var vId = (cmbVozilo!.SelectedItem as ComboItem)?.Id ?? 0;
                    a.VoziloId = vId > 0 ? vId : null;
                })),
                "ZDRAVSTVENA" => DTOManager.dodajZdravstvenuStetu(KopirajU<ZdravstvenaStetaBasic>(baza, z =>
                {
                    z.Dijagnoza = txtDijagnoza!.Text.Trim(); z.MedicinskaDocumentacija = txtMedDok!.Text.Trim();
                    z.ZdravstvenaUstanova = txtZdravUstanova!.Text.Trim();
                    var lId = (cmbLekar!.SelectedItem as ComboItem)?.Id ?? 0;
                    z.LekarId = lId > 0 ? lId : null;
                })),
                "IMOVINSKA" => DTOManager.dodajImovinskuStetu(KopirajU<ImovinskStetaBasic>(baza, im =>
                {
                    im.ProcenaOstecenja = txtProcenaOstecenja!.Text.Trim(); im.IzvodjacSanacije = txtIzvodjacSanacije!.Text.Trim();
                })),
                _ => DTOManager.dodajStetu(baza)
            };

            if (noviId <= 0) return;

            foreach (var l in _pendingLica) { l.StetaId = noviId; DTOManager.dodajOstecenoLice(l); }
            foreach (var p in _pendingPredmeti) { p.StetaId = noviId; DTOManager.dodajOsteceniPredmet(p); }
            foreach (var pr in _pendingProcene) { pr.StetaId = noviId; DTOManager.dodajProcenu(pr); }

            DialogResult = DialogResult.OK;
            Close();
        }

        private static T KopirajU<T>(StetaBasic baza, Action<T> podesi) where T : StetaBasic, new()
        {
            var t = new T
            {
                BrojStete = baza.BrojStete, VrstaStete = baza.VrstaStete,
                PolisaId = baza.PolisaId, PodnosilacId = baza.PodnosilacId,
                DatumNastanka = baza.DatumNastanka, Lokacija = baza.Lokacija,
                OpisDogodjaja = baza.OpisDogodjaja, ProcenjeniIznos = baza.ProcenjeniIznos,
                Valuta = baza.Valuta, Status = baza.Status
            };
            podesi(t);
            return t;
        }
    }
}
