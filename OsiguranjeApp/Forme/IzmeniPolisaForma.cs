using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // Izmeni polisu - tip se ne menja posle kreiranja, ali sva tip-specificna polja i
    // Osiguranici/Dodatna pokrića/Korisnici isplate su izmenljivi direktno ovde (isto nacelo
    // kao Dodaj, samo sto se ovde upisi u bazu dese odmah, ne cekaju cuvanje cele forme).
    public partial class IzmeniPolisaForma : Form
    {
        private readonly PolisaPregled _polisa; // konkretan podtip (ZivotnoPregled, AutoPolisaPregled...)
        private readonly string _tip;

        public IzmeniPolisaForma(object detalji)
        {
            _polisa = (PolisaPregled)detalji;
            _tip = _polisa.TipOsiguranja ?? "";
            UcitajComboove();
            InitializeComponent();
            PopuniFormu();
        }

        private void UcitajComboove()
        {
            _klijenti = DTOManager.vratiSveKlijente();
            _vozila = DTOManager.vratiSvaVozila();
            _nekretnine = DTOManager.vratiSveNekretnine();
            _pokretnaImovina = DTOManager.vratiSvuPokretnuImovinu();
            _usevi = DTOManager.vratiSveUseve();
            _zivotinje = DTOManager.vratiSveZivotinje();
        }

        private GroupBox NapraviBaznaPoljaGrupa()
        {
            var grp = new GroupBox { Text = $"Osnovni podaci — {NazivTipa(_tip)}", Width = 500, Height = 300 };
            var tbl = UiHelper.NapraviLayout(7);
            tbl.Dock = DockStyle.Fill;

            cmbUgovarac = UiHelper.DodajComboRed(tbl, 0, "Ugovarač *:");
            cmbUgovarac.Items.Add(new ComboItem(0, "-- Izaberi --"));
            foreach (var k in _klijenti) cmbUgovarac.Items.Add(new ComboItem(k.KlijentId, k.Naziv));

            cmbAgent = UiHelper.DodajComboRed(tbl, 1, "Agent:");
            cmbAgent.Items.Add(new ComboItem(0, "-- Bez agenta --"));
            foreach (var a in DTOManager.vratiSveAgente()) cmbAgent.Items.Add(new ComboItem(a.OsobljeId, $"{a.Ime} {a.Prezime}"));

            dtpPocetka  = UiHelper.DodajDTPRed(tbl, 2, "Datum početka *:");
            dtpIsteka   = UiHelper.DodajDTPRed(tbl, 3, "Datum isteka *:");
            txtPremija  = UiHelper.DodajRed(tbl, 4, "Osnovna premija *:");
            cmbNacin    = UiHelper.DodajComboRed(tbl, 5, "Način plaćanja:");
            cmbNacin.Items.AddRange(new object[] { "JEDNOKRATNO", "MESECNO", "KVARTAL", "POLUGODISNJE", "GODISNJE" });
            cmbStatus   = UiHelper.DodajComboRed(tbl, 6, "Status:");
            cmbStatus.Items.AddRange(new object[] { "AKTIVNA", "ISTEKLA", "RASKINUTA", "MIROVANJE", "OBNOVLJENA" });

            grp.Controls.Add(tbl);
            return grp;
        }

        private static string NazivTipa(string tip) => tip switch
        {
            "ZIVOTNO" => "Životno osiguranje", "ZDRAVSTVENO" => "Zdravstveno osiguranje",
            "AUTO" => "Auto osiguranje", "IMOVINSKO" => "Imovinsko osiguranje",
            "PUTNO" => "Putno osiguranje", "POLJOPRIVREDNO" => "Poljoprivredno osiguranje",
            "ODGOVORNOST" => "Osiguranje odgovornosti", "SPECIJALIZOVANO" => "Specijalizovano osiguranje",
            _ => tip
        };

        // Racuna se iz tbl.RowCount (36px po redu + Padding(16) gore/dole iz NapraviLayout), ne iz
        // tbl.Height - u trenutku poziva grp jos nije dobio konacnu visinu pa Dock.Fill jos ne odslikava
        // stvarnu visinu tabele, sto bi pomerilo listu i preklopilo je sa poljima iznad.
        // Kad tbl deli GroupBox sa rucno pozicioniranim listboxovima ispod sebe, NE sme da ostane
        // Dock.Fill (razvuklo bi ga preko cele grupe i preklopilo/prekrilo listbox koji je dodat posle
        // njega). Ucvrscuje ga na tacnu prirodnu visinu (36px po redu + Padding 16 gore/dole) na vrhu grupe.
        private static void UcvrstiVisinu(TableLayoutPanel tbl)
        {
            tbl.Dock = DockStyle.None;
            tbl.Location = new Point(0, 0);
            tbl.Size = new Size(500, tbl.RowCount * 36 + 32);
        }

        private ListBox DodajListBoxIspod(GroupBox grp, TableLayoutPanel tbl, string labelTekst, IEnumerable<ComboItem> stavke, int yOffset = -1)
        {
            int y = yOffset >= 0 ? yOffset : tbl.RowCount * 36 + 32 + 4;
            var lbl = new Label { Text = labelTekst, Location = new Point(4, y), AutoSize = true };
            var lst = new ListBox { Location = new Point(4, y + 18), Size = new Size(470, 70), SelectionMode = SelectionMode.MultiExtended };
            foreach (var it in stavke) lst.Items.Add(it);
            grp.Controls.Add(lbl);
            grp.Controls.Add(lst);
            return lst;
        }

        private static void OznaciIzabrane(ListBox lst, IEnumerable<int> ids)
        {
            var skup = ids.ToHashSet();
            for (int i = 0; i < lst.Items.Count; i++)
                if (lst.Items[i] is ComboItem it && skup.Contains(it.Id))
                    lst.SetSelected(i, true);
        }

        private GroupBox NapraviTipSpecificnaGrupa()
        {
            var grp = new GroupBox { Text = "Dodatni podaci", Width = 500 };
            TableLayoutPanel tbl;

            switch (_tip)
            {
                case "ZIVOTNO":
                    tbl = UiHelper.NapraviLayout(2);
                    txtSumaOsiguranja = UiHelper.DodajRed(tbl, 0, "Suma osiguranja *:");
                    cmbTipIsplate = UiHelper.DodajComboRed(tbl, 1, "Tip isplate:");
                    cmbTipIsplate.Items.AddRange(new object[] { "JEDNOKRATNA", "MESECNA_RENTA", "KOMBINOVANA" });
                    grp.Controls.Add(tbl);
                    grp.Height = 110;
                    break;

                case "ZDRAVSTVENO":
                    tbl = UiHelper.NapraviLayout(6);
                    txtMrezaUstanova = UiHelper.DodajRed(tbl, 0, "Mreža ustanova:");
                    txtPokrica = UiHelper.DodajRed(tbl, 1, "Obuhvaćena pokrića:");
                    txtLimitSpecijalista = UiHelper.DodajRed(tbl, 2, "Limit — specijalisti:");
                    txtLimitStomatologa = UiHelper.DodajRed(tbl, 3, "Limit — stomatolog:");
                    txtLimitBolnickih = UiHelper.DodajRed(tbl, 4, "Limit — bolničke int.:");
                    txtLimitBolnickiDan = UiHelper.DodajRed(tbl, 5, "Limit — bolnički dan:");
                    grp.Controls.Add(tbl);
                    grp.Height = 250;
                    break;

                case "AUTO":
                    tbl = UiHelper.NapraviLayout(3);
                    cmbVozilo = UiHelper.DodajComboRed(tbl, 0, "Vozilo *:");
                    cmbVozilo.Items.Add(new ComboItem(0, "-- Izaberi vozilo --"));
                    foreach (var v in _vozila) cmbVozilo.Items.Add(new ComboItem(v.VoziloId, $"{v.Registracija} — {v.Marka} {v.Model}"));
                    txtBonusMalus = UiHelper.DodajRed(tbl, 1, "Bonus-malus klasa:");
                    txtTeritorijalno = UiHelper.DodajRed(tbl, 2, "Teritorijalno važenje:");
                    grp.Controls.Add(tbl);
                    UcvrstiVisinu(tbl);
                    lstVozaci = DodajListBoxIspod(grp, tbl, "Dodatni vozači (Ctrl+klik):", _klijenti.Select(k => new ComboItem(k.KlijentId, k.Naziv)));
                    OznaciIzabrane(lstVozaci, DTOManager.vratiVozaceAutoOsiguranja(_polisa.PolisaId));
                    grp.Height = 260;
                    break;

                case "IMOVINSKO":
                    tbl = UiHelper.NapraviLayout(1);
                    txtVrsteRizika = UiHelper.DodajRed(tbl, 0, "Vrste rizika:");
                    grp.Controls.Add(tbl);
                    UcvrstiVisinu(tbl);
                    int imBazaY = tbl.RowCount * 36 + 32;
                    lstNekretnine = DodajListBoxIspod(grp, tbl, "Nekretnine (Ctrl+klik):", _nekretnine.Select(n => new ComboItem(n.NekretninaId, n.Adresa)), yOffset: imBazaY);
                    lstPokretnaImovina = DodajListBoxIspod(grp, tbl, "Pokretna imovina (Ctrl+klik):", _pokretnaImovina.Select(p => new ComboItem(p.PokretnaImovinaId, p.Naziv)), yOffset: imBazaY + 90);
                    var imDet = (ImovinskoPregled)_polisa;
                    OznaciIzabrane(lstNekretnine, imDet.NekretnineIds);
                    OznaciIzabrane(lstPokretnaImovina, imDet.PokretnaImovinaIds);
                    grp.Height = imBazaY + 220;
                    break;

                case "PUTNO":
                    tbl = UiHelper.NapraviLayout(3);
                    txtDestinacije = UiHelper.DodajRed(tbl, 0, "Destinacije:");
                    dtpPolazak = UiHelper.DodajDTPRed(tbl, 1, "Datum polaska *:");
                    dtpPovratak = UiHelper.DodajDTPRed(tbl, 2, "Datum povratka *:");
                    grp.Controls.Add(tbl);
                    UcvrstiVisinu(tbl);
                    lstOsiguranaLica = DodajListBoxIspod(grp, tbl, "Osigurana lica (Ctrl+klik):", _klijenti.Select(k => new ComboItem(k.KlijentId, k.Naziv)));
                    OznaciIzabrane(lstOsiguranaLica, ((PutnoPregled)_polisa).OsiguranaLicaIds);
                    grp.Height = 270;
                    break;

                case "POLJOPRIVREDNO":
                    tbl = UiHelper.NapraviLayout(0);
                    grp.Controls.Add(tbl);
                    UcvrstiVisinu(tbl);
                    int poljBazaY = tbl.RowCount * 36 + 32;
                    lstUsevi = DodajListBoxIspod(grp, tbl, "Usevi (Ctrl+klik):", _usevi.Select(u => new ComboItem(u.UsevId, $"{u.Vrsta} ({u.Lokacija})")), yOffset: poljBazaY);
                    lstZivotinje = DodajListBoxIspod(grp, tbl, "Životinje (Ctrl+klik):", _zivotinje.Select(z => new ComboItem(z.ZivotinjaId, $"{z.Vrsta} ({z.Lokacija})")), yOffset: poljBazaY + 90);
                    var poljDet = (PoljoprivrednoPregled)_polisa;
                    OznaciIzabrane(lstUsevi, poljDet.UseviIds);
                    OznaciIzabrane(lstZivotinje, poljDet.ZivotinjeIds);
                    grp.Height = poljBazaY + 220;
                    break;

                case "ODGOVORNOST":
                    tbl = UiHelper.NapraviLayout(2);
                    txtVrstaOdgovornosti = UiHelper.DodajRed(tbl, 0, "Vrsta odgovornosti:");
                    txtLimitOdgovornosti = UiHelper.DodajRed(tbl, 1, "Limit odgovornosti:");
                    grp.Controls.Add(tbl);
                    grp.Height = 110;
                    break;

                case "SPECIJALIZOVANO":
                    tbl = UiHelper.NapraviLayout(2);
                    txtNazivSpecijalizacije = UiHelper.DodajRed(tbl, 0, "Naziv specijalizacije *:");
                    txtOpisUslova = UiHelper.DodajRed(tbl, 1, "Opis uslova:");
                    grp.Controls.Add(tbl);
                    grp.Height = 110;
                    break;

                default:
                    grp.Height = 10;
                    break;
            }

            return grp;
        }

        private void PopuniFormu()
        {
            dtpPocetka.Value       = _polisa.DatumPocetka;
            dtpIsteka.Value        = _polisa.DatumIsteka;
            txtPremija.Text        = _polisa.OsnovnaPremija.ToString("F2");
            cmbNacin.SelectedItem  = _polisa.NacinPlacanja;
            cmbStatus.SelectedItem = _polisa.Status;

            for (int i = 0; i < cmbUgovarac.Items.Count; i++)
                if ((cmbUgovarac.Items[i] as ComboItem)?.Id == _polisa.UgovaracId)
                { cmbUgovarac.SelectedIndex = i; break; }

            if (_polisa.AgentId.HasValue)
                for (int i = 0; i < cmbAgent.Items.Count; i++)
                    if ((cmbAgent.Items[i] as ComboItem)?.Id == _polisa.AgentId.Value)
                    { cmbAgent.SelectedIndex = i; break; }
            else cmbAgent.SelectedIndex = 0;

            switch (_polisa)
            {
                case ZivotnoPregled z:
                    txtSumaOsiguranja!.Text = z.SumaOsiguranja.ToString("F2");
                    cmbTipIsplate!.SelectedItem = z.TipIsplate;
                    break;
                case ZdravstvenoPregled zd:
                    txtMrezaUstanova!.Text = zd.MrezaUstanova ?? ""; txtPokrica!.Text = zd.Pokrica ?? "";
                    txtLimitSpecijalista!.Text = zd.LimitSpecijalista.ToString("F2");
                    txtLimitStomatologa!.Text = zd.LimitStomatologa.ToString("F2");
                    txtLimitBolnickih!.Text = zd.LimitBolnickih.ToString("F2");
                    txtLimitBolnickiDan!.Text = zd.LimitBolnickiDan.ToString("F2");
                    break;
                case AutoPolisaPregled a:
                    for (int i = 0; i < cmbVozilo!.Items.Count; i++)
                        if ((cmbVozilo.Items[i] as ComboItem)?.Id == a.VoziloId) { cmbVozilo.SelectedIndex = i; break; }
                    txtBonusMalus!.Text = a.BonusMalusKlasa ?? ""; txtTeritorijalno!.Text = a.TeritorijanoVazenje ?? "";
                    break;
                case ImovinskoPregled im:
                    txtVrsteRizika!.Text = im.VrsteRizika ?? "";
                    break;
                case PutnoPregled pu:
                    txtDestinacije!.Text = pu.Destinacije ?? "";
                    dtpPolazak!.Value = pu.DatumPolaska; dtpPovratak!.Value = pu.DatumPovratka;
                    break;
                case OdgovornostPregled od:
                    txtVrstaOdgovornosti!.Text = od.VrstaOdgovornosti ?? "";
                    txtLimitOdgovornosti!.Text = od.LimitOdgovornosti?.ToString("F2") ?? "";
                    break;
                case SpecijalizovanoPregled sp:
                    txtNazivSpecijalizacije!.Text = sp.NazivSpecijalizacije ?? ""; txtOpisUslova!.Text = sp.OpisUslova ?? "";
                    break;
            }
        }

        // ---------- Osiguranici (upis odmah, polisa vec postoji) ----------

        private GroupBox NapraviOsiguraniciGrupa()
        {
            var grp = new GroupBox { Text = "Osiguranici", Width = 500, Height = 178 };

            var lblHint = new Label
            {
                Text = "Trenutni osiguranici (dupli klik za uklanjanje):",
                Location = new Point(10, 20), AutoSize = true,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Italic), ForeColor = UiHelper.Siva
            };
            lstOsiguranici = new ListBox { Location = new Point(10, 38), Size = new Size(478, 64) };
            lstOsiguranici.DoubleClick += (s, e) =>
            {
                if (lstOsiguranici.SelectedItem is not UlogaKlijentaBasic u) return;
                if (MessageBox.Show($"Ukloniti osiguranika \"{u}\"?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiUlogu(u.UlogaId))) return;
                UcitajOsiguranike();
            };
            grp.Controls.Add(lblHint);
            grp.Controls.Add(lstOsiguranici);
            UcitajOsiguranike();

            cmbOsigKlijent = new ComboBox { Location = new Point(10, 112), Size = new Size(220, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbOsigKlijent.Items.Add(new ComboItem(0, "-- nije registrovan klijent --"));
            foreach (var k in _klijenti) cmbOsigKlijent.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbOsigKlijent.SelectedIndex = 0;
            txtOsigImePrezime = new TextBox { Location = new Point(240, 112), Size = new Size(158, 26), PlaceholderText = "ili slobodno ime i prezime" };
            var btnDodaj = UiHelper.NapraviDugme("➕ Dodaj", UiHelper.Zelena, 80);
            btnDodaj.Location = new Point(408, 111);

            btnDodaj.Click += (s, e) =>
            {
                var izabran = cmbOsigKlijent.SelectedItem as ComboItem;
                string ime = txtOsigImePrezime.Text.Trim();
                if ((izabran == null || izabran.Id == 0) && string.IsNullOrEmpty(ime))
                { MessageBox.Show("Izaberite klijenta ili unesite ime i prezime.", "Validacija"); return; }
                var dto = new UlogaKlijentaBasic
                {
                    PolisaId = _polisa.PolisaId,
                    KlijentId = izabran?.Id > 0 ? izabran.Id : null,
                    ImePrezime = izabran?.Id > 0 ? null : ime,
                    TipUloge = "OSIGURANIK"
                };
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajUlogu(dto))) return;
                cmbOsigKlijent.SelectedIndex = 0; txtOsigImePrezime.Clear();
                UcitajOsiguranike();
            };

            grp.Controls.Add(cmbOsigKlijent);
            grp.Controls.Add(txtOsigImePrezime);
            grp.Controls.Add(btnDodaj);
            return grp;
        }

        private void UcitajOsiguranike()
        {
            lstOsiguranici.Items.Clear();
            foreach (var u in DTOManager.vratiUlogeZaPolisu(_polisa.PolisaId)) lstOsiguranici.Items.Add(u);
        }

        // ---------- Dodatna pokrića (upis odmah) ----------

        private GroupBox NapraviDodatnaPokricaGrupa()
        {
            var grp = new GroupBox { Text = "Dodatna pokrića", Width = 500, Height = 180 };

            var lblHint = new Label
            {
                Text = "Trenutna pokrića (dupli klik za uklanjanje):",
                Location = new Point(10, 20), AutoSize = true,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Italic), ForeColor = UiHelper.Siva
            };
            lstPokrica = new ListBox { Location = new Point(10, 38), Size = new Size(478, 60) };
            lstPokrica.DoubleClick += (s, e) =>
            {
                if (lstPokrica.SelectedItem is not DodatnoPokrBasic p) return;
                if (MessageBox.Show($"Ukloniti pokriće \"{p.Naziv}\"?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiDodatnoPokrice(p.PokriceId))) return;
                UcitajPokrica();
            };
            grp.Controls.Add(lblHint);
            grp.Controls.Add(lstPokrica);
            UcitajPokrica();

            txtPkNaziv = new TextBox { Location = new Point(10, 106), Size = new Size(108, 26), PlaceholderText = "Naziv *" };
            txtPkOpis = new TextBox { Location = new Point(124, 106), Size = new Size(108, 26), PlaceholderText = "Opis" };
            txtPkLimit = new TextBox { Location = new Point(238, 106), Size = new Size(68, 26), PlaceholderText = "Limit" };
            txtPkFransiza = new TextBox { Location = new Point(312, 106), Size = new Size(68, 26), PlaceholderText = "Franš. %" };
            txtPkPremija = new TextBox { Location = new Point(386, 106), Size = new Size(68, 26), PlaceholderText = "Premija" };
            var btnDodaj = UiHelper.NapraviDugme("➕ Dodaj pokriće", UiHelper.Zelena, 150);
            btnDodaj.Location = new Point(10, 140);

            btnDodaj.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPkNaziv.Text))
                { MessageBox.Show("Naziv pokrića je obavezan.", "Validacija"); return; }
                decimal? ParsiOpc(TextBox t) => decimal.TryParse(t.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : (decimal?)null;
                var dto = new DodatnoPokrBasic
                {
                    PolisaId = _polisa.PolisaId,
                    Naziv = txtPkNaziv.Text.Trim(), Opis = txtPkOpis.Text.Trim(),
                    LimitPokrića = ParsiOpc(txtPkLimit), Fransiza = ParsiOpc(txtPkFransiza) ?? 0, DodatnaPremija = ParsiOpc(txtPkPremija) ?? 0
                };
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajDodatnoPokrice(dto))) return;
                txtPkNaziv.Clear(); txtPkOpis.Clear(); txtPkLimit.Clear(); txtPkFransiza.Clear(); txtPkPremija.Clear();
                UcitajPokrica();
            };

            grp.Controls.AddRange(new Control[] { txtPkNaziv, txtPkOpis, txtPkLimit, txtPkFransiza, txtPkPremija, btnDodaj });
            return grp;
        }

        private void UcitajPokrica()
        {
            lstPokrica.Items.Clear();
            foreach (var p in DTOManager.vratiPolisu(_polisa.PolisaId).DodatnaPokrića) lstPokrica.Items.Add(p);
        }

        // ---------- Korisnici isplate (upis odmah, samo ZIVOTNO) ----------

        private GroupBox NapraviKorisniciIsplateGrupa()
        {
            var grp = new GroupBox { Text = "Korisnici isplate", Width = 500, Height = 178 };

            var lblHint = new Label
            {
                Text = "Trenutni korisnici (zbir procenata treba da bude 100%, dupli klik za uklanjanje):",
                Location = new Point(10, 20), AutoSize = true,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Italic), ForeColor = UiHelper.Siva
            };
            lstKorisnici = new ListBox { Location = new Point(10, 38), Size = new Size(478, 64) };
            lstKorisnici.DoubleClick += (s, e) =>
            {
                if (lstKorisnici.SelectedItem is not KorisnikIsplateBasic k) return;
                if (MessageBox.Show($"Ukloniti korisnika \"{k}\"?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiKorisnikaIsplate(k.KorisnikId))) return;
                UcitajKorisnike();
            };
            grp.Controls.Add(lblHint);
            grp.Controls.Add(lstKorisnici);
            if (_tip == "ZIVOTNO") UcitajKorisnike();

            cmbKiKlijent = new ComboBox { Location = new Point(10, 112), Size = new Size(200, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKiKlijent.Items.Add(new ComboItem(0, "-- nije registrovan --"));
            foreach (var k in _klijenti) cmbKiKlijent.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbKiKlijent.SelectedIndex = 0;
            txtKiImePrezime = new TextBox { Location = new Point(216, 112), Size = new Size(160, 26), PlaceholderText = "ili ime i prezime" };
            txtKiProcenat = new TextBox { Location = new Point(382, 112), Size = new Size(70, 26), PlaceholderText = "Udeo %" };
            var btnDodaj = UiHelper.NapraviDugme("➕ Dodaj", UiHelper.Zelena, 80);
            btnDodaj.Location = new Point(10, 144);

            btnDodaj.Click += (s, e) =>
            {
                var izabran = cmbKiKlijent.SelectedItem as ComboItem;
                string ime = txtKiImePrezime.Text.Trim();
                if ((izabran == null || izabran.Id == 0) && string.IsNullOrEmpty(ime))
                { MessageBox.Show("Izaberite klijenta ili unesite ime i prezime.", "Validacija"); return; }
                if (!decimal.TryParse(txtKiProcenat.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal procenat))
                { MessageBox.Show("Procenat udela mora biti broj.", "Validacija"); return; }
                var dto = new KorisnikIsplateBasic
                {
                    PolisaId = _polisa.PolisaId,
                    KlijentId = izabran?.Id > 0 ? izabran.Id : null,
                    ImePrezime = izabran?.Id > 0 ? null : ime,
                    ProcenatUdela = procenat
                };
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajKorisnikaIsplate(dto))) return;
                cmbKiKlijent.SelectedIndex = 0; txtKiImePrezime.Clear(); txtKiProcenat.Clear();
                UcitajKorisnike();
            };

            grp.Controls.AddRange(new Control[] { cmbKiKlijent, txtKiImePrezime, txtKiProcenat, btnDodaj });
            return grp;
        }

        private void UcitajKorisnike()
        {
            lstKorisnici.Items.Clear();
            foreach (var k in DTOManager.vratiKorisnikeIsplateZaPolisu(_polisa.PolisaId)) lstKorisnici.Items.Add(k);
        }

        // ---------- Cuvanje osnovnih + tip-specificnih polja ----------

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if ((cmbUgovarac.SelectedItem as ComboItem)?.Id == 0)
            { MessageBox.Show("Izaberite ugovarača.", "Validacija"); return; }
            if (!decimal.TryParse(txtPremija.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal prem) || prem <= 0)
            { MessageBox.Show("Premija mora biti pozitivan broj.", "Validacija"); return; }
            if (dtpIsteka.Value <= dtpPocetka.Value)
            { MessageBox.Show("Datum isteka mora biti posle početka.", "Validacija"); return; }

            var agent = cmbAgent.SelectedItem as ComboItem;
            _polisa.DatumPocetka   = dtpPocetka.Value.Date;
            _polisa.DatumIsteka    = dtpIsteka.Value.Date;
            _polisa.OsnovnaPremija = prem;
            _polisa.NacinPlacanja  = cmbNacin.SelectedItem?.ToString();
            _polisa.Status         = cmbStatus.SelectedItem?.ToString();
            _polisa.UgovaracId     = ((ComboItem)cmbUgovarac.SelectedItem!).Id;
            _polisa.AgentId        = agent?.Id > 0 ? agent.Id : (int?)null;

            decimal? ParsiDecOpc(string txt) => decimal.TryParse(txt.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : (decimal?)null;
            bool uspeh;

            switch (_polisa)
            {
                case ZivotnoPregled z:
                    if (!decimal.TryParse(txtSumaOsiguranja!.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal suma) || suma <= 0)
                    { MessageBox.Show("Suma osiguranja mora biti pozitivan broj.", "Validacija"); return; }
                    z.SumaOsiguranja = suma; z.TipIsplate = cmbTipIsplate!.SelectedItem?.ToString();
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajZivotnoOsiguranje(z));
                    break;

                case ZdravstvenoPregled zd:
                    zd.MrezaUstanova = txtMrezaUstanova!.Text.Trim(); zd.Pokrica = txtPokrica!.Text.Trim();
                    zd.LimitSpecijalista = ParsiDecOpc(txtLimitSpecijalista!.Text) ?? 0;
                    zd.LimitStomatologa = ParsiDecOpc(txtLimitStomatologa!.Text) ?? 0;
                    zd.LimitBolnickih = ParsiDecOpc(txtLimitBolnickih!.Text) ?? 0;
                    zd.LimitBolnickiDan = ParsiDecOpc(txtLimitBolnickiDan!.Text) ?? 0;
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajZdravstvenoOsiguranje(zd));
                    break;

                case AutoPolisaPregled a:
                    if ((cmbVozilo!.SelectedItem as ComboItem)?.Id is not int voziloId || voziloId == 0)
                    { MessageBox.Show("Izaberite vozilo.", "Validacija"); return; }
                    a.VoziloId = voziloId; a.BonusMalusKlasa = txtBonusMalus!.Text.Trim(); a.TeritorijanoVazenje = txtTeritorijalno!.Text.Trim();
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajAutoOsiguranje(a));
                    if (uspeh) DTOManager.azurirajVozaceAutoOsiguranja(_polisa.PolisaId, lstVozaci!.SelectedItems.Cast<ComboItem>().Select(c => c.Id).ToList());
                    break;

                case ImovinskoPregled im:
                    im.VrsteRizika = txtVrsteRizika!.Text.Trim();
                    im.NekretnineIds.Clear(); im.NekretnineIds.AddRange(lstNekretnine!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    im.PokretnaImovinaIds.Clear(); im.PokretnaImovinaIds.AddRange(lstPokretnaImovina!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajImovinskoOsiguranje(im));
                    break;

                case PutnoPregled pu:
                    if (dtpPovratak!.Value <= dtpPolazak!.Value)
                    { MessageBox.Show("Datum povratka mora biti posle datuma polaska.", "Validacija"); return; }
                    pu.Destinacije = txtDestinacije!.Text.Trim(); pu.DatumPolaska = dtpPolazak.Value.Date; pu.DatumPovratka = dtpPovratak.Value.Date;
                    pu.OsiguranaLicaIds.Clear(); pu.OsiguranaLicaIds.AddRange(lstOsiguranaLica!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajPutnoOsiguranje(pu));
                    break;

                case PoljoprivrednoPregled polj:
                    polj.UseviIds.Clear(); polj.UseviIds.AddRange(lstUsevi!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    polj.ZivotinjeIds.Clear(); polj.ZivotinjeIds.AddRange(lstZivotinje!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajPoljoprivrednoOsiguranje(polj));
                    break;

                case OdgovornostPregled od:
                    od.VrstaOdgovornosti = txtVrstaOdgovornosti!.Text.Trim(); od.LimitOdgovornosti = ParsiDecOpc(txtLimitOdgovornosti!.Text);
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajOsiguranjeOdgovornosti(od));
                    break;

                case SpecijalizovanoPregled sp:
                    if (string.IsNullOrWhiteSpace(txtNazivSpecijalizacije!.Text))
                    { MessageBox.Show("Naziv specijalizacije je obavezan.", "Validacija"); return; }
                    sp.NazivSpecijalizacije = txtNazivSpecijalizacije.Text.Trim(); sp.OpisUslova = txtOpisUslova!.Text.Trim();
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajSpecijalizovanoOsiguranje(sp));
                    break;

                default:
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.azurirajPolisu(new PolisaBasic
                    {
                        PolisaId = _polisa.PolisaId, BrojPolise = _polisa.BrojPolise, TipOsiguranja = _polisa.TipOsiguranja,
                        DatumPocetka = _polisa.DatumPocetka, DatumIsteka = _polisa.DatumIsteka,
                        OsnovnaPremija = _polisa.OsnovnaPremija, Valuta = _polisa.Valuta,
                        NacinPlacanja = _polisa.NacinPlacanja, Status = _polisa.Status,
                        UgovaracId = _polisa.UgovaracId, AgentId = _polisa.AgentId
                    }));
                    break;
            }

            if (!uspeh) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
