using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // Izmeni stetu - tip se ne menja, ali sva tip-specificna polja i Ostecena lica/Predmeti/Procene
    // su izmenljivi direktno ovde (upisi u bazu se desavaju odmah, polisa vec postoji).
    public partial class IzmeniSteteForma : Form
    {
        private readonly StetaBasic _steta;
        private readonly string _vrsta;
        private List<KlijentPregled> _klijenti = new();
        private readonly bool _smeProcena = SesijaKorisnik.ImaUlogu("ADMIN", "PROCENITELJ");

        public IzmeniSteteForma(object detalji)
        {
            _steta = (StetaBasic)detalji;
            _vrsta = _steta.VrstaStete ?? "";
            _klijenti = DTOManager.vratiSveKlijente();
            InitializeComponent();
            PopuniFormu();
        }

        private GroupBox NapraviBaznaPoljaGrupa()
        {
            var grp = new GroupBox { Text = $"Osnovni podaci — {_steta.VrstaStete}", Width = 500, Height = 260 };
            var tbl = UiHelper.NapraviLayout(6);
            tbl.Dock = DockStyle.Fill;

            dtpNastanka = UiHelper.DodajDTPRed(tbl, 0, "Datum nastanka *:");
            txtLokacija = UiHelper.DodajRed(tbl, 1, "Lokacija:");
            txtOpis = UiHelper.DodajRed(tbl, 2, "Opis događaja:");
            txtIznos = UiHelper.DodajRed(tbl, 3, "Procenjeni iznos:");
            cmbValuta = UiHelper.DodajComboRed(tbl, 4, "Valuta:");
            cmbValuta.Items.AddRange(new object[] { "RSD", "EUR", "USD" });
            cmbStatus = UiHelper.DodajComboRed(tbl, 5, "Status:");
            cmbStatus.Items.AddRange(new object[] { "PRIJAVLJENA", "U_OBRADI", "U_PROCENI", "ODOBRENA", "ODBIJENA", "ISPLACENA", "ZATVORENA" });

            grp.Controls.Add(tbl);
            return grp;
        }

        private GroupBox NapraviTipSpecificnaGrupa()
        {
            var grp = new GroupBox { Text = "Dodatni podaci", Width = 500 };
            if (_vrsta is not ("AUTO" or "ZDRAVSTVENA" or "IMOVINSKA")) { grp.Height = 5; return grp; }
            TableLayoutPanel tbl;

            switch (_vrsta)
            {
                case "AUTO":
                    tbl = UiHelper.NapraviLayout(3);
                    cmbVozilo = UiHelper.DodajComboRed(tbl, 0, "Vozilo:");
                    cmbVozilo.Items.Add(new ComboItem(0, "-- nije poznato --"));
                    foreach (var v in DTOManager.vratiSvaVozila()) cmbVozilo.Items.Add(new ComboItem(v.VoziloId, $"{v.Registracija} — {v.Marka} {v.Model}"));
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
            return grp;
        }

        private void PopuniFormu()
        {
            dtpNastanka.Value = _steta.DatumNastanka;
            txtLokacija.Text = _steta.Lokacija ?? "";
            txtOpis.Text = _steta.OpisDogodjaja ?? "";
            txtIznos.Text = _steta.ProcenjeniIznos?.ToString("F2") ?? "";
            cmbValuta.SelectedItem = _steta.Valuta ?? "RSD";
            cmbStatus.SelectedItem = _steta.Status;

            switch (_steta)
            {
                case AutoStetaBasic a:
                    for (int i = 0; i < cmbVozilo!.Items.Count; i++)
                        if ((cmbVozilo.Items[i] as ComboItem)?.Id == (a.VoziloId ?? 0)) { cmbVozilo.SelectedIndex = i; break; }
                    if (cmbVozilo.SelectedIndex < 0) cmbVozilo.SelectedIndex = 0;
                    txtZapisnik!.Text = a.ZapisnikPolicije ?? ""; txtServis!.Text = a.Servis ?? "";
                    break;
                case ZdravstvenaStetaBasic z:
                    txtDijagnoza!.Text = z.Dijagnoza ?? ""; txtMedDok!.Text = z.MedicinskaDocumentacija ?? "";
                    txtZdravUstanova!.Text = z.ZdravstvenaUstanova ?? "";
                    for (int i = 0; i < cmbLekar!.Items.Count; i++)
                        if ((cmbLekar.Items[i] as ComboItem)?.Id == (z.LekarId ?? 0)) { cmbLekar.SelectedIndex = i; break; }
                    if (cmbLekar.SelectedIndex < 0) cmbLekar.SelectedIndex = 0;
                    break;
                case ImovinskStetaBasic im:
                    txtProcenaOstecenja!.Text = im.ProcenaOstecenja ?? ""; txtIzvodjacSanacije!.Text = im.IzvodjacSanacije ?? "";
                    break;
            }

            UcitajLica(); UcitajPredmete();
            if (_smeProcena) UcitajProcene();
        }

        private GroupBox NapraviOstecenaLicaGrupa()
        {
            var grp = new GroupBox { Text = "Oštećena lica", Width = 500, Height = 150 };
            lstLica = new ListBox { Location = new Point(8, 20), Size = new Size(484, 55) };
            grp.Controls.Add(lstLica);

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
                    StetaId = _steta.StetaId,
                    KlijentId = izabran?.Id > 0 ? izabran.Id : null,
                    ImePrezime = izabran?.Id > 0 ? null : ime,
                    OpisPovrede = txtOlOpis.Text.Trim(), IznosNaknade = ParsiOpc(txtOlIznos.Text)
                };
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajOstecenoLice(dto))) return;
                cmbOlKlijent.SelectedIndex = 0; txtOlImePrezime.Clear(); txtOlOpis.Clear(); txtOlIznos.Clear();
                UcitajLica();
            };
            btnUkloni.Click += (s, e) =>
            {
                if (lstLica.SelectedItem is not OstecenLiceBasic l) return;
                if (MessageBox.Show($"Ukloniti \"{l}\"?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiOstecenoLice(l.OstecenLiceId))) return;
                UcitajLica();
            };

            grp.Controls.AddRange(new Control[] { cmbOlKlijent, txtOlImePrezime, txtOlOpis, txtOlIznos, btnDodaj, btnUkloni });
            return grp;
        }

        private void UcitajLica()
        {
            lstLica.Items.Clear();
            foreach (var l in DTOManager.vratiOstecenaLicaZaStetu(_steta.StetaId)) lstLica.Items.Add(l);
        }

        private GroupBox NapraviOsteceniPredmetiGrupa()
        {
            var grp = new GroupBox { Text = "Oštećeni predmeti", Width = 500, Height = 150 };
            lstPredmeti = new ListBox { Location = new Point(8, 20), Size = new Size(484, 55) };
            grp.Controls.Add(lstPredmeti);

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
                var dto = new OsteceniPredmetBasic { StetaId = _steta.StetaId, TipPredmeta = txtOpTip.Text.Trim(), OpisOstecenja = txtOpOpis.Text.Trim(), ProcenjeniIznos = ParsiOpc(txtOpIznos.Text) };
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajOsteceniPredmet(dto))) return;
                txtOpTip.Clear(); txtOpOpis.Clear(); txtOpIznos.Clear();
                UcitajPredmete();
            };
            btnUkloni.Click += (s, e) =>
            {
                if (lstPredmeti.SelectedItem is not OsteceniPredmetBasic p) return;
                if (MessageBox.Show($"Ukloniti \"{p}\"?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiOsteceniPredmet(p.OsteceniPredmetId))) return;
                UcitajPredmete();
            };

            grp.Controls.AddRange(new Control[] { txtOpTip, txtOpOpis, txtOpIznos, btnDodaj, btnUkloni });
            return grp;
        }

        private void UcitajPredmete()
        {
            lstPredmeti.Items.Clear();
            foreach (var p in DTOManager.vratiOsteceniPredmetiZaStetu(_steta.StetaId)) lstPredmeti.Items.Add(p);
        }

        private GroupBox NapraviProceneGrupa()
        {
            var grp = new GroupBox { Text = "Procene štete", Width = 500, Height = 180 };
            lstProcene = new ListBox { Location = new Point(8, 20), Size = new Size(484, 55) };
            grp.Controls.Add(lstProcene);

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
                    StetaId = _steta.StetaId, ProceniteljId = procId, DatumProc = DateTime.Today,
                    MetodProc = txtPrMetod.Text.Trim(), Nalaz = txtPrNalaz.Text.Trim(),
                    ProcenjeniIznos = iznos, Preporuka = txtPrPreporuka.Text.Trim()
                };
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajProcenu(dto))) return;
                cmbPrProcenitelj.SelectedIndex = 0; txtPrMetod.Clear(); txtPrNalaz.Clear(); txtPrIznos.Clear(); txtPrPreporuka.Clear();
                UcitajProcene();
            };
            btnUkloni.Click += (s, e) =>
            {
                if (lstProcene.SelectedItem is not ProcenaStetaBasic p) return;
                if (MessageBox.Show("Ukloniti izabranu procenu?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiProcenu(p.ProcenaId))) return;
                UcitajProcene();
            };

            grp.Controls.AddRange(new Control[] { cmbPrProcenitelj, txtPrMetod, txtPrIznos, txtPrNalaz, txtPrPreporuka, btnDodaj, btnUkloni });
            return grp;
        }

        private void UcitajProcene()
        {
            lstProcene.Items.Clear();
            foreach (var p in DTOManager.vratiStetuDetaljno(_steta.StetaId) is StetaBasic sb ? sb.ProceneSteta : Enumerable.Empty<ProcenaStetaBasic>())
                lstProcene.Items.Add($"{p.ProceniteljIme}: {p.ProcenjeniIznos:N2} — {p.Nalaz}");
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            decimal? iznos = null;
            if (!string.IsNullOrWhiteSpace(txtIznos.Text) &&
                decimal.TryParse(txtIznos.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal iz))
                iznos = iz;

            _steta.DatumNastanka = dtpNastanka.Value.Date;
            _steta.Lokacija = txtLokacija.Text.Trim();
            _steta.OpisDogodjaja = txtOpis.Text.Trim();
            _steta.ProcenjeniIznos = iznos;
            _steta.Valuta = cmbValuta.SelectedItem?.ToString();
            _steta.Status = cmbStatus.SelectedItem?.ToString();

            bool uspeh = _steta switch
            {
                AutoStetaBasic a => SacuvajAuto(a),
                ZdravstvenaStetaBasic z => SacuvajZdravstvenu(z),
                ImovinskStetaBasic im => SacuvajImovinsku(im),
                _ => UiHelper.PokusajAkciju(() => DTOManager.azurirajStetu(_steta))
            };

            if (!uspeh) return;
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool SacuvajAuto(AutoStetaBasic a)
        {
            a.ZapisnikPolicije = txtZapisnik!.Text.Trim(); a.Servis = txtServis!.Text.Trim();
            var vId = (cmbVozilo!.SelectedItem as ComboItem)?.Id ?? 0;
            a.VoziloId = vId > 0 ? vId : null;
            return UiHelper.PokusajAkciju(() => DTOManager.azurirajAutoStetu(a));
        }

        private bool SacuvajZdravstvenu(ZdravstvenaStetaBasic z)
        {
            z.Dijagnoza = txtDijagnoza!.Text.Trim(); z.MedicinskaDocumentacija = txtMedDok!.Text.Trim();
            z.ZdravstvenaUstanova = txtZdravUstanova!.Text.Trim();
            var lId = (cmbLekar!.SelectedItem as ComboItem)?.Id ?? 0;
            z.LekarId = lId > 0 ? lId : null;
            return UiHelper.PokusajAkciju(() => DTOManager.azurirajZdravstvenuStetu(z));
        }

        private bool SacuvajImovinsku(ImovinskStetaBasic im)
        {
            im.ProcenaOstecenja = txtProcenaOstecenja!.Text.Trim(); im.IzvodjacSanacije = txtIzvodjacSanacije!.Text.Trim();
            return UiHelper.PokusajAkciju(() => DTOManager.azurirajImovinskuStetu(im));
        }
    }
}
