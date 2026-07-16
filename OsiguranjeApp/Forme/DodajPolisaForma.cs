using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // Nova polisa - svih 8 tipova osiguranja sa sopstvenim poljima, plus Osiguranici/Dodatna
    // pokrića/Korisnici isplate direktno u istoj formi (isto nacelo kao web klijent: pending
    // liste koje se upisuju tek posto se bazni zapis polise sacuva i dobije PolisaId).
    public partial class DodajPolisaForma : Form
    {
        public DodajPolisaForma()
        {
            UcitajComboove();
            InitializeComponent();
            AzurirajTipSpecificnaPolja();
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
            // Jedan TableLayoutPanel za svih 10 redova (umesto ranije dva odvojena panela sa
            // mesanim Dock.Fill/Dock.Top rezimom) - to je uzrokovalo da drugi panel (Nacin
            // placanja/Status) legne PREKO prvog umesto ispod njega, jer Dock.Top uvek ima
            // prioritet nad Dock.Fill bez obzira na redosled dodavanja u Controls.
            var grp = new GroupBox { Text = "Osnovni podaci", Width = 500, Height = 400 };
            var tbl = UiHelper.NapraviLayout(10);

            cmbTip = UiHelper.DodajComboRed(tbl, 0, "Tip osiguranja *:");
            cmbTip.Items.AddRange(new object[] { "ZIVOTNO", "ZDRAVSTVENO", "AUTO", "IMOVINSKO", "PUTNO", "POLJOPRIVREDNO", "ODGOVORNOST", "SPECIJALIZOVANO" });
            cmbTip.SelectedIndex = 0;
            cmbTip.SelectedIndexChanged += (s, e) => { txtBroj.Text = GenerisiBroj(); AzurirajTipSpecificnaPolja(); AzurirajVidljivostOsiguranikaKorisnika(); };

            txtBroj      = UiHelper.DodajRed(tbl, 1, "Broj polise *:");
            cmbUgovarac  = UiHelper.DodajComboRed(tbl, 2, "Ugovarač *:");
            cmbUgovarac.Items.Add(new ComboItem(0, "-- Izaberi klijenta --"));
            foreach (var k in _klijenti) cmbUgovarac.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbUgovarac.SelectedIndex = 0;

            cmbAgent     = UiHelper.DodajComboRed(tbl, 3, "Agent:");
            cmbAgent.Items.Add(new ComboItem(0, "-- Bez agenta --"));
            foreach (var a in DTOManager.vratiSveAgente()) cmbAgent.Items.Add(new ComboItem(a.OsobljeId, $"{a.Ime} {a.Prezime}"));
            cmbAgent.SelectedIndex = 0;

            dtpPocetka   = UiHelper.DodajDTPRed(tbl, 4, "Datum početka *:");
            dtpIsteka    = UiHelper.DodajDTPRed(tbl, 5, "Datum isteka *:");
            dtpIsteka.Value = DateTime.Today.AddYears(1);
            txtPremija   = UiHelper.DodajRed(tbl, 6, "Osnovna premija *:");
            cmbValuta    = UiHelper.DodajComboRed(tbl, 7, "Valuta:");
            cmbValuta.Items.AddRange(new object[] { "RSD", "EUR", "USD" });
            cmbValuta.SelectedIndex = 0;

            cmbNacin     = UiHelper.DodajComboRed(tbl, 8, "Način plaćanja:");
            cmbNacin.Items.AddRange(new object[] { "JEDNOKRATNO", "MESECNO", "KVARTAL", "POLUGODISNJE", "GODISNJE" });
            cmbNacin.SelectedIndex = 1;
            cmbStatus    = UiHelper.DodajComboRed(tbl, 9, "Status:");
            cmbStatus.Items.AddRange(new object[] { "AKTIVNA", "ISTEKLA", "RASKINUTA", "MIROVANJE" });
            cmbStatus.SelectedIndex = 0;

            grp.Controls.Add(tbl);
            txtBroj.Text = GenerisiBroj();
            return grp;
        }

        private string GenerisiBroj()
        {
            var tip  = cmbTip.SelectedItem?.ToString() ?? "POL";
            var pref = tip.Length >= 4 ? tip.Substring(0, 4) : tip;
            return $"POL-{pref}-{DateTime.Now.Year}-{new Random().Next(100, 999)}";
        }

        // ---------- Tip-specificna polja (rebuild pri promeni tipa) ----------

        private void AzurirajTipSpecificnaPolja()
        {
            pnlTipSpecificno.Controls.Clear();
            txtSumaOsiguranja = null; cmbTipIsplate = null;
            txtMrezaUstanova = txtPokrica = txtLimitSpecijalista = txtLimitStomatologa = txtLimitBolnickih = txtLimitBolnickiDan = null;
            cmbVozilo = null; txtBonusMalus = txtTeritorijalno = null; lstVozaci = null;
            txtVrsteRizika = null; lstNekretnine = lstPokretnaImovina = null;
            txtDestinacije = null; dtpPolazak = dtpPovratak = null; lstOsiguranaLica = null;
            lstUsevi = lstZivotinje = null;
            txtVrstaOdgovornosti = txtLimitOdgovornosti = null;
            txtNazivSpecijalizacije = txtOpisUslova = null;

            string tip = cmbTip.SelectedItem?.ToString() ?? "";
            var grp = new GroupBox { Text = $"{NazivTipa(tip)} — dodatni podaci", Width = 500 };
            TableLayoutPanel tbl;

            switch (tip)
            {
                case "ZIVOTNO":
                    tbl = UiHelper.NapraviLayout(2);
                    txtSumaOsiguranja = UiHelper.DodajRed(tbl, 0, "Suma osiguranja *:");
                    cmbTipIsplate = UiHelper.DodajComboRed(tbl, 1, "Tip isplate:");
                    cmbTipIsplate.Items.AddRange(new object[] { "JEDNOKRATNA", "MESECNA_RENTA", "KOMBINOVANA" });
                    cmbTipIsplate.SelectedIndex = 0;
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
                    cmbVozilo.SelectedIndex = 0;
                    txtBonusMalus = UiHelper.DodajRed(tbl, 1, "Bonus-malus klasa:");
                    txtTeritorijalno = UiHelper.DodajRed(tbl, 2, "Teritorijalno važenje:");
                    grp.Controls.Add(tbl);
                    UcvrstiVisinu(tbl);
                    lstVozaci = DodajListBoxIspod(grp, tbl, "Dodatni vozači (Ctrl+klik):", _klijenti.Select(k => new ComboItem(k.KlijentId, k.Naziv)));
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
                    grp.Height = imBazaY + 220;
                    break;

                case "PUTNO":
                    tbl = UiHelper.NapraviLayout(3);
                    txtDestinacije = UiHelper.DodajRed(tbl, 0, "Destinacije:");
                    dtpPolazak = UiHelper.DodajDTPRed(tbl, 1, "Datum polaska *:");
                    dtpPovratak = UiHelper.DodajDTPRed(tbl, 2, "Datum povratka *:");
                    dtpPovratak.Value = DateTime.Today.AddDays(14);
                    grp.Controls.Add(tbl);
                    UcvrstiVisinu(tbl);
                    lstOsiguranaLica = DodajListBoxIspod(grp, tbl, "Osigurana lica (Ctrl+klik):", _klijenti.Select(k => new ComboItem(k.KlijentId, k.Naziv)));
                    grp.Height = 270;
                    break;

                case "POLJOPRIVREDNO":
                    tbl = UiHelper.NapraviLayout(0);
                    grp.Controls.Add(tbl);
                    UcvrstiVisinu(tbl);
                    int poljBazaY = tbl.RowCount * 36 + 32;
                    lstUsevi = DodajListBoxIspod(grp, tbl, "Usevi (Ctrl+klik):", _usevi.Select(u => new ComboItem(u.UsevId, $"{u.Vrsta} ({u.Lokacija})")), yOffset: poljBazaY);
                    lstZivotinje = DodajListBoxIspod(grp, tbl, "Životinje (Ctrl+klik):", _zivotinje.Select(z => new ComboItem(z.ZivotinjaId, $"{z.Vrsta} ({z.Lokacija})")), yOffset: poljBazaY + 90);
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
            }

            pnlTipSpecificno.Controls.Add(grp);
            pnlTipSpecificno.Height = grp.Height;
        }

        private static string NazivTipa(string tip) => tip switch
        {
            "ZIVOTNO" => "Životno osiguranje", "ZDRAVSTVENO" => "Zdravstveno osiguranje",
            "AUTO" => "Auto osiguranje", "IMOVINSKO" => "Imovinsko osiguranje",
            "PUTNO" => "Putno osiguranje", "POLJOPRIVREDNO" => "Poljoprivredno osiguranje",
            "ODGOVORNOST" => "Osiguranje odgovornosti", "SPECIJALIZOVANO" => "Specijalizovano osiguranje",
            _ => tip
        };

        // Kad tbl deli GroupBox sa rucno pozicioniranim listboxovima ispod sebe, NE sme da ostane
        // Dock.Fill (razvuklo bi ga preko cele grupe i preklopilo/prekrilo listbox koji je dodat posle
        // njega). Ucvrscuje ga na tacnu prirodnu visinu (36px po redu + Padding 16 gore/dole) na vrhu grupe.
        private static void UcvrstiVisinu(TableLayoutPanel tbl)
        {
            tbl.Dock = DockStyle.None;
            tbl.Location = new Point(0, 0);
            tbl.Size = new Size(500, tbl.RowCount * 36 + 32);
        }

        // Pomocna funkcija: dodaje labelovan multi-select ListBox ispod postojeceg tbl-a u istom GroupBox-u.
        // Racuna se iz tbl.RowCount (36px po redu + Padding(16) gore/dole iz NapraviLayout), ne iz
        // tbl.Height - u trenutku poziva grp jos nije dobio konacnu visinu pa Dock.Fill jos ne odslikava
        // stvarnu visinu tabele, sto bi pomerilo listu i preklopilo je sa poljima iznad.
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

        // ---------- Osiguranici (fold-in) ----------

        private GroupBox NapraviOsiguraniciGrupa()
        {
            var grp = new GroupBox { Text = "Osiguranici", Width = 500, Height = 178 };

            var lblHint = new Label
            {
                Text = "Dodati osiguranici (dupli klik za uklanjanje):",
                Location = new Point(10, 20), AutoSize = true,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Italic), ForeColor = UiHelper.Siva
            };
            lstPendingOsiguranici = new ListBox { Location = new Point(10, 38), Size = new Size(478, 64) };
            lstPendingOsiguranici.DoubleClick += (s, e) =>
            {
                int idx = lstPendingOsiguranici.SelectedIndex;
                if (idx < 0) return;
                _pendingOsiguranici.RemoveAt(idx);
                lstPendingOsiguranici.Items.RemoveAt(idx);
            };
            grp.Controls.Add(lblHint);
            grp.Controls.Add(lstPendingOsiguranici);

            cmbOsigKlijent = new ComboBox { Location = new Point(10, 112), Size = new Size(220, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbOsigKlijent.Items.Add(new ComboItem(0, "-- nije registrovan klijent --"));
            foreach (var k in _klijenti) cmbOsigKlijent.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbOsigKlijent.SelectedIndex = 0;
            txtOsigImePrezime = new TextBox { Location = new Point(240, 112), Size = new Size(158, 26), PlaceholderText = "ili slobodno ime i prezime" };
            var btnDodajOsig = UiHelper.NapraviDugme("➕ Dodaj", UiHelper.Zelena, 80);
            btnDodajOsig.Location = new Point(408, 111);
            btnDodajOsig.Click += (s, e) =>
            {
                var izabran = cmbOsigKlijent.SelectedItem as ComboItem;
                string ime = txtOsigImePrezime.Text.Trim();
                if ((izabran == null || izabran.Id == 0) && string.IsNullOrEmpty(ime))
                { MessageBox.Show("Izaberite klijenta ili unesite ime i prezime.", "Validacija"); return; }
                var dto = new UlogaKlijentaBasic
                {
                    KlijentId = izabran?.Id > 0 ? izabran.Id : null,
                    KlijentNaziv = izabran?.Id > 0 ? izabran.Tekst : null,
                    ImePrezime = izabran?.Id > 0 ? null : ime,
                    TipUloge = "OSIGURANIK"
                };
                _pendingOsiguranici.Add(dto);
                lstPendingOsiguranici.Items.Add(dto.KlijentNaziv ?? dto.ImePrezime ?? "");
                cmbOsigKlijent.SelectedIndex = 0; txtOsigImePrezime.Clear();
            };
            grp.Controls.Add(cmbOsigKlijent);
            grp.Controls.Add(txtOsigImePrezime);
            grp.Controls.Add(btnDodajOsig);
            return grp;
        }

        private void AzurirajVidljivostOsiguranikaKorisnika()
        {
            string tip = cmbTip.SelectedItem?.ToString() ?? "";
            grpOsiguranici.Visible = tip != "PUTNO"; // Putno vec ima "Osigurana lica" kao svoj predmet
            grpKorisnici.Visible = tip == "ZIVOTNO";
        }

        // ---------- Dodatna pokrića (fold-in, uvek vidljivo) ----------

        private GroupBox NapraviDodatnaPokricaGrupa()
        {
            var grp = new GroupBox { Text = "Dodatna pokrića", Width = 500, Height = 180 };

            var lblHint = new Label
            {
                Text = "Dodata pokrića (dupli klik za uklanjanje):",
                Location = new Point(10, 20), AutoSize = true,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Italic), ForeColor = UiHelper.Siva
            };
            lstPendingPokrica = new ListBox { Location = new Point(10, 38), Size = new Size(478, 60) };
            lstPendingPokrica.DoubleClick += (s, e) =>
            {
                int idx = lstPendingPokrica.SelectedIndex;
                if (idx < 0) return;
                _pendingPokrica.RemoveAt(idx);
                lstPendingPokrica.Items.RemoveAt(idx);
            };
            grp.Controls.Add(lblHint);
            grp.Controls.Add(lstPendingPokrica);

            txtPkNaziv = new TextBox { Location = new Point(10, 106), Size = new Size(108, 26), PlaceholderText = "Naziv *" };
            txtPkOpis = new TextBox { Location = new Point(124, 106), Size = new Size(108, 26), PlaceholderText = "Opis" };
            txtPkLimit = new TextBox { Location = new Point(238, 106), Size = new Size(68, 26), PlaceholderText = "Limit" };
            txtPkFransiza = new TextBox { Location = new Point(312, 106), Size = new Size(68, 26), PlaceholderText = "Franš. %" };
            txtPkPremija = new TextBox { Location = new Point(386, 106), Size = new Size(68, 26), PlaceholderText = "Premija" };
            var btnDodajPk = UiHelper.NapraviDugme("➕ Dodaj pokriće", UiHelper.Zelena, 150);
            btnDodajPk.Location = new Point(10, 140);

            btnDodajPk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPkNaziv.Text))
                { MessageBox.Show("Naziv pokrića je obavezan.", "Validacija"); return; }
                decimal? ParsiOpc(TextBox t) => decimal.TryParse(t.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : (decimal?)null;
                var dto = new DodatnoPokrBasic
                {
                    Naziv = txtPkNaziv.Text.Trim(), Opis = txtPkOpis.Text.Trim(),
                    LimitPokrića = ParsiOpc(txtPkLimit),
                    Fransiza = ParsiOpc(txtPkFransiza) ?? 0,
                    DodatnaPremija = ParsiOpc(txtPkPremija) ?? 0
                };
                _pendingPokrica.Add(dto);
                lstPendingPokrica.Items.Add($"{dto.Naziv} — +{dto.DodatnaPremija:N2}");
                txtPkNaziv.Clear(); txtPkOpis.Clear(); txtPkLimit.Clear(); txtPkFransiza.Clear(); txtPkPremija.Clear();
            };

            grp.Controls.AddRange(new Control[] { txtPkNaziv, txtPkOpis, txtPkLimit, txtPkFransiza, txtPkPremija, btnDodajPk });
            return grp;
        }

        // ---------- Korisnici isplate (fold-in, samo ZIVOTNO) ----------

        private GroupBox NapraviKorisniciIsplateGrupa()
        {
            var grp = new GroupBox { Text = "Korisnici isplate", Width = 500, Height = 178 };

            var lblHint = new Label
            {
                Text = "Dodati korisnici (zbir procenata treba da bude 100%, dupli klik za uklanjanje):",
                Location = new Point(10, 20), AutoSize = true,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Italic), ForeColor = UiHelper.Siva
            };
            lstPendingKorisnici = new ListBox { Location = new Point(10, 38), Size = new Size(478, 64) };
            lstPendingKorisnici.DoubleClick += (s, e) =>
            {
                int idx = lstPendingKorisnici.SelectedIndex;
                if (idx < 0) return;
                _pendingKorisnici.RemoveAt(idx);
                lstPendingKorisnici.Items.RemoveAt(idx);
            };
            grp.Controls.Add(lblHint);
            grp.Controls.Add(lstPendingKorisnici);

            cmbKiKlijent = new ComboBox { Location = new Point(10, 112), Size = new Size(200, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKiKlijent.Items.Add(new ComboItem(0, "-- nije registrovan --"));
            foreach (var k in _klijenti) cmbKiKlijent.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbKiKlijent.SelectedIndex = 0;
            txtKiImePrezime = new TextBox { Location = new Point(216, 112), Size = new Size(160, 26), PlaceholderText = "ili ime i prezime" };
            txtKiProcenat = new TextBox { Location = new Point(382, 112), Size = new Size(70, 26), PlaceholderText = "Udeo %" };
            var btnDodajKi = UiHelper.NapraviDugme("➕ Dodaj", UiHelper.Zelena, 80);
            btnDodajKi.Location = new Point(10, 144);

            btnDodajKi.Click += (s, e) =>
            {
                var izabran = cmbKiKlijent.SelectedItem as ComboItem;
                string ime = txtKiImePrezime.Text.Trim();
                if ((izabran == null || izabran.Id == 0) && string.IsNullOrEmpty(ime))
                { MessageBox.Show("Izaberite klijenta ili unesite ime i prezime.", "Validacija"); return; }
                if (!decimal.TryParse(txtKiProcenat.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal procenat))
                { MessageBox.Show("Procenat udela mora biti broj.", "Validacija"); return; }
                var dto = new KorisnikIsplateBasic
                {
                    KlijentId = izabran?.Id > 0 ? izabran.Id : null,
                    KlijentNaziv = izabran?.Id > 0 ? izabran.Tekst : null,
                    ImePrezime = izabran?.Id > 0 ? null : ime,
                    ProcenatUdela = procenat
                };
                _pendingKorisnici.Add(dto);
                lstPendingKorisnici.Items.Add($"{dto.KlijentNaziv ?? dto.ImePrezime} — {dto.ProcenatUdela:N2}%");
                cmbKiKlijent.SelectedIndex = 0; txtKiImePrezime.Clear(); txtKiProcenat.Clear();
            };

            grp.Controls.AddRange(new Control[] { cmbKiKlijent, txtKiImePrezime, txtKiProcenat, btnDodajKi });
            return grp;
        }

        // ---------- Cuvanje ----------

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBroj.Text))
            { MessageBox.Show("Broj polise je obavezan.", "Validacija"); return; }
            if ((cmbUgovarac.SelectedItem as ComboItem)?.Id == 0)
            { MessageBox.Show("Izaberite ugovarača.", "Validacija"); return; }
            if (!decimal.TryParse(txtPremija.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal prem) || prem <= 0)
            { MessageBox.Show("Premija mora biti pozitivan broj.", "Validacija"); return; }
            if (dtpIsteka.Value <= dtpPocetka.Value)
            { MessageBox.Show("Datum isteka mora biti posle datuma početka.", "Validacija"); return; }

            string tip = cmbTip.SelectedItem?.ToString() ?? "";
            var agent = cmbAgent.SelectedItem as ComboItem;
            var baza = new PolisaPregled
            {
                BrojPolise    = txtBroj.Text.Trim(),
                TipOsiguranja = tip,
                DatumPocetka  = dtpPocetka.Value.Date,
                DatumIsteka   = dtpIsteka.Value.Date,
                OsnovnaPremija = prem,
                Valuta        = cmbValuta.SelectedItem?.ToString(),
                NacinPlacanja = cmbNacin.SelectedItem?.ToString(),
                Status        = cmbStatus.SelectedItem?.ToString(),
                UgovaracId    = ((ComboItem)cmbUgovarac.SelectedItem!).Id,
                AgentId       = agent?.Id > 0 ? agent.Id : (int?)null
            };

            decimal? ParsiDecOpc(string txt) => decimal.TryParse(txt.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : (decimal?)null;

            int noviId = 0;
            switch (tip)
            {
                case "ZIVOTNO":
                    if (!decimal.TryParse(txtSumaOsiguranja!.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal suma) || suma <= 0)
                    { MessageBox.Show("Suma osiguranja mora biti pozitivan broj.", "Validacija"); return; }
                    noviId = DTOManager.dodajZivotnoOsiguranje(KopirajU<ZivotnoPregled>(baza, z => { z.SumaOsiguranja = suma; z.TipIsplate = cmbTipIsplate!.SelectedItem?.ToString(); }));
                    break;

                case "ZDRAVSTVENO":
                    noviId = DTOManager.dodajZdravstvenoOsiguranje(KopirajU<ZdravstvenoPregled>(baza, z =>
                    {
                        z.MrezaUstanova = txtMrezaUstanova!.Text.Trim(); z.Pokrica = txtPokrica!.Text.Trim();
                        z.LimitSpecijalista = ParsiDecOpc(txtLimitSpecijalista!.Text) ?? 0;
                        z.LimitStomatologa = ParsiDecOpc(txtLimitStomatologa!.Text) ?? 0;
                        z.LimitBolnickih = ParsiDecOpc(txtLimitBolnickih!.Text) ?? 0;
                        z.LimitBolnickiDan = ParsiDecOpc(txtLimitBolnickiDan!.Text) ?? 0;
                    }));
                    break;

                case "AUTO":
                    if ((cmbVozilo!.SelectedItem as ComboItem)?.Id is not int voziloId || voziloId == 0)
                    { MessageBox.Show("Izaberite vozilo.", "Validacija"); return; }
                    noviId = DTOManager.dodajAutoOsiguranje(KopirajU<AutoPolisaPregled>(baza, a =>
                    {
                        a.VoziloId = voziloId; a.BonusMalusKlasa = txtBonusMalus!.Text.Trim(); a.TeritorijanoVazenje = txtTeritorijalno!.Text.Trim();
                    }));
                    if (noviId > 0 && lstVozaci!.SelectedItems.Count > 0)
                        DTOManager.azurirajVozaceAutoOsiguranja(noviId, lstVozaci.SelectedItems.Cast<ComboItem>().Select(c => c.Id).ToList());
                    break;

                case "IMOVINSKO":
                    var imDto = KopirajU<ImovinskoPregled>(baza, im => im.VrsteRizika = txtVrsteRizika!.Text.Trim());
                    imDto.NekretnineIds.AddRange(lstNekretnine!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    imDto.PokretnaImovinaIds.AddRange(lstPokretnaImovina!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    noviId = DTOManager.dodajImovinskoOsiguranje(imDto);
                    break;

                case "PUTNO":
                    if (dtpPovratak!.Value <= dtpPolazak!.Value)
                    { MessageBox.Show("Datum povratka mora biti posle datuma polaska.", "Validacija"); return; }
                    var puDto = KopirajU<PutnoPregled>(baza, pu =>
                    {
                        pu.Destinacije = txtDestinacije!.Text.Trim(); pu.DatumPolaska = dtpPolazak.Value.Date; pu.DatumPovratka = dtpPovratak.Value.Date;
                    });
                    puDto.OsiguranaLicaIds.AddRange(lstOsiguranaLica!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    noviId = DTOManager.dodajPutnoOsiguranje(puDto);
                    break;

                case "POLJOPRIVREDNO":
                    var poljDto = KopirajU<PoljoprivrednoPregled>(baza, _ => { });
                    poljDto.UseviIds.AddRange(lstUsevi!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    poljDto.ZivotinjeIds.AddRange(lstZivotinje!.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    noviId = DTOManager.dodajPoljoprivrednoOsiguranje(poljDto);
                    break;

                case "ODGOVORNOST":
                    noviId = DTOManager.dodajOsiguranjeOdgovornosti(KopirajU<OdgovornostPregled>(baza, o =>
                    {
                        o.VrstaOdgovornosti = txtVrstaOdgovornosti!.Text.Trim(); o.LimitOdgovornosti = ParsiDecOpc(txtLimitOdgovornosti!.Text);
                    }));
                    break;

                case "SPECIJALIZOVANO":
                    if (string.IsNullOrWhiteSpace(txtNazivSpecijalizacije!.Text))
                    { MessageBox.Show("Naziv specijalizacije je obavezan.", "Validacija"); return; }
                    noviId = DTOManager.dodajSpecijalizovanoOsiguranje(KopirajU<SpecijalizovanoPregled>(baza, sp =>
                    {
                        sp.NazivSpecijalizacije = txtNazivSpecijalizacije.Text.Trim(); sp.OpisUslova = txtOpisUslova!.Text.Trim();
                    }));
                    break;
            }

            if (noviId <= 0) return;

            foreach (var o in _pendingOsiguranici) { o.PolisaId = noviId; DTOManager.dodajUlogu(o); }
            foreach (var p in _pendingPokrica) { p.PolisaId = noviId; DTOManager.dodajDodatnoPokrice(p); }
            foreach (var k in _pendingKorisnici) { k.PolisaId = noviId; DTOManager.dodajKorisnikaIsplate(k); }

            DialogResult = DialogResult.OK;
            Close();
        }

        // Kopira zajednicka (bazna) polja u novu instancu T (podtip PolisaPregled), pa primenjuje dodatna podesavanja.
        private static T KopirajU<T>(PolisaPregled baza, Action<T> podesi) where T : PolisaPregled, new()
        {
            var t = new T
            {
                BrojPolise = baza.BrojPolise, TipOsiguranja = baza.TipOsiguranja,
                DatumPocetka = baza.DatumPocetka, DatumIsteka = baza.DatumIsteka,
                OsnovnaPremija = baza.OsnovnaPremija, Valuta = baza.Valuta,
                NacinPlacanja = baza.NacinPlacanja, Status = baza.Status,
                UgovaracId = baza.UgovaracId, AgentId = baza.AgentId
            };
            podesi(t);
            return t;
        }
    }
}
