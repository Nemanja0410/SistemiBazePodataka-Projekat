using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class DodajPolisaForma : Form
    {
        private TextBox       txtBroj = null!, txtPremija = null!;
        private ComboBox      cmbTip = null!, cmbValuta = null!, cmbNacin = null!, cmbStatus = null!;
        private ComboBox      cmbUgovarac = null!, cmbAgent = null!;
        private DateTimePicker dtpPocetka = null!, dtpIsteka = null!;
        private Button        btnSacuvaj = null!, btnOdustani = null!;
        private TableLayoutPanel tbl = null!;

        // Polja specifična za POLJOPRIVREDNO / ODGOVORNOST / SPECIJALIZOVANO
        // (ostalih 5 tipova i dalje koristi samo zajednička polja polise, kao i do sad).
        private TextBox  txtVrstaOdgovornosti = null!, txtLimitOdgovornosti = null!;
        private TextBox  txtNazivSpecijalizacije = null!, txtOpisUslova = null!;
        private ListBox  lstUsevi = null!, lstZivotinje = null!;

        public DodajPolisaForma()
        {
            InitializeComponent();
            UcitajComboove();
        }

        private void InitializeComponent()
        {
            this.Text            = "Nova polisa osiguranja";
            this.Size            = new Size(490, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;
            this.AutoScroll      = true;

            tbl = UiHelper.NapraviLayout(17);

            cmbTip     = UiHelper.DodajComboRed(tbl, 0, "Tip osiguranja *:");
            cmbTip.Items.AddRange(new[] { "ZIVOTNO","ZDRAVSTVENO","IMOVINSKO","AUTO","PUTNO","POLJOPRIVREDNO","ODGOVORNOST","SPECIJALIZOVANO" });
            cmbTip.SelectedIndex = 0;
            cmbTip.SelectedIndexChanged += (s, e) => { txtBroj.Text = GenerisiBroj(); AzurirajVidljivostDodatnihPolja(); };

            txtBroj      = UiHelper.DodajRed(tbl, 1, "Broj polise *:");
            cmbUgovarac  = UiHelper.DodajComboRed(tbl, 2, "Ugovarač *:");
            cmbAgent     = UiHelper.DodajComboRed(tbl, 3, "Agent:");
            dtpPocetka   = UiHelper.DodajDTPRed(tbl, 4, "Datum početka *:");
            dtpIsteka    = UiHelper.DodajDTPRed(tbl, 5, "Datum isteka *:");
            dtpIsteka.Value = DateTime.Today.AddYears(1);
            txtPremija   = UiHelper.DodajRed(tbl, 6, "Osnovna premija *:");
            cmbValuta    = UiHelper.DodajComboRed(tbl, 7, "Valuta:");
            cmbValuta.Items.AddRange(new[] { "RSD", "EUR", "USD" });
            cmbValuta.SelectedIndex = 0;
            cmbNacin     = UiHelper.DodajComboRed(tbl, 8, "Način plaćanja:");
            cmbNacin.Items.AddRange(new[] { "JEDNOKRATNO","MESECNO","KVARTAL","POLUGODISNJE","GODISNJE" });
            cmbNacin.SelectedIndex = 1;
            cmbStatus    = UiHelper.DodajComboRed(tbl, 9, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVNA","ISTEKLA","RASKINUTA","MIROVANJE" });
            cmbStatus.SelectedIndex = 0;

            txtVrstaOdgovornosti   = UiHelper.DodajRed(tbl, 10, "Vrsta odgovornosti:");
            txtLimitOdgovornosti   = UiHelper.DodajRed(tbl, 11, "Limit odgovornosti:");
            txtNazivSpecijalizacije = UiHelper.DodajRed(tbl, 12, "Naziv specijalizacije *:");
            txtOpisUslova          = UiHelper.DodajRed(tbl, 13, "Opis uslova:");
            lstUsevi               = DodajListBoxRed(tbl, 14, "Usevi (Ctrl+klik):");
            lstZivotinje           = DodajListBoxRed(tbl, 15, "Životinje (Ctrl+klik):");

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 16);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
            AzurirajVidljivostDodatnihPolja();
        }

        private ListBox DodajListBoxRed(TableLayoutPanel tabela, int red, string labelTekst)
        {
            tabela.Controls.Add(new Label
            {
                Text      = labelTekst,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding   = new Padding(0, 0, 8, 0)
            }, 0, red);
            var lst = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.MultiExtended };
            tabela.Controls.Add(lst, 1, red);
            return lst;
        }

        private void AzurirajVidljivostDodatnihPolja()
        {
            string tip = cmbTip.SelectedItem?.ToString() ?? "";
            bool jeOdgovornost   = tip == "ODGOVORNOST";
            bool jeSpecijalizovano = tip == "SPECIJALIZOVANO";
            bool jePoljoprivredno = tip == "POLJOPRIVREDNO";

            PostaviRed(10, jeOdgovornost, 36);
            PostaviRed(11, jeOdgovornost, 36);
            PostaviRed(12, jeSpecijalizovano, 36);
            PostaviRed(13, jeSpecijalizovano, 36);
            PostaviRed(14, jePoljoprivredno, 70);
            PostaviRed(15, jePoljoprivredno, 70);

            int visinaBaza = 470;
            if (jeOdgovornost) visinaBaza += 72;
            if (jeSpecijalizovano) visinaBaza += 72;
            if (jePoljoprivredno) visinaBaza += 140;
            this.Height = Math.Min(visinaBaza, 760);
        }

        private void PostaviRed(int red, bool vidljivo, int visinaKadJeVidljivo)
        {
            tbl.GetControlFromPosition(0, red)!.Visible = vidljivo;
            tbl.GetControlFromPosition(1, red)!.Visible = vidljivo;
            tbl.RowStyles[red].Height = vidljivo ? visinaKadJeVidljivo : 0;
        }

        private string GenerisiBroj()
        {
            var tip  = cmbTip.SelectedItem?.ToString() ?? "POL";
            var pref = tip.Length >= 4 ? tip.Substring(0, 4) : tip;
            return $"POL-{pref}-{DateTime.Now.Year}-{new Random().Next(100, 999)}";
        }

        private void UcitajComboove()
        {
            cmbUgovarac.Items.Add(new ComboItem(0, "-- Izaberi klijenta --"));
            foreach (var k in DTOManager.vratiSveKlijente())
                cmbUgovarac.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbUgovarac.SelectedIndex = 0;

            cmbAgent.Items.Add(new ComboItem(0, "-- Bez agenta --"));
            foreach (var a in DTOManager.vratiSveAgente())
                cmbAgent.Items.Add(new ComboItem(a.OsobljeId, $"{a.Ime} {a.Prezime}"));
            cmbAgent.SelectedIndex = 0;

            foreach (var u in DTOManager.vratiSveUseve())
                lstUsevi.Items.Add(new ComboItem(u.UsevId, $"{u.Vrsta} ({u.Lokacija})"));
            foreach (var z in DTOManager.vratiSveZivotinje())
                lstZivotinje.Items.Add(new ComboItem(z.ZivotinjaId, $"{z.Vrsta} ({z.Lokacija})"));

            txtBroj.Text = GenerisiBroj();
        }

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

            if (tip == "SPECIJALIZOVANO" && string.IsNullOrWhiteSpace(txtNazivSpecijalizacije.Text))
            { MessageBox.Show("Naziv specijalizacije je obavezan.", "Validacija"); return; }

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

            bool uspeh;
            switch (tip)
            {
                case "ODGOVORNOST":
                    decimal? limit = null;
                    if (!string.IsNullOrWhiteSpace(txtLimitOdgovornosti.Text) &&
                        decimal.TryParse(txtLimitOdgovornosti.Text.Replace(",", "."),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out decimal lim))
                        limit = lim;
                    var dtoOdg = new OdgovornostPregled
                    {
                        BrojPolise = baza.BrojPolise, TipOsiguranja = baza.TipOsiguranja,
                        DatumPocetka = baza.DatumPocetka, DatumIsteka = baza.DatumIsteka,
                        OsnovnaPremija = baza.OsnovnaPremija, Valuta = baza.Valuta,
                        NacinPlacanja = baza.NacinPlacanja, Status = baza.Status,
                        UgovaracId = baza.UgovaracId, AgentId = baza.AgentId,
                        VrstaOdgovornosti = txtVrstaOdgovornosti.Text.Trim(),
                        LimitOdgovornosti = limit
                    };
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.dodajOsiguranjeOdgovornosti(dtoOdg));
                    break;

                case "SPECIJALIZOVANO":
                    var dtoSpec = new SpecijalizovanoPregled
                    {
                        BrojPolise = baza.BrojPolise, TipOsiguranja = baza.TipOsiguranja,
                        DatumPocetka = baza.DatumPocetka, DatumIsteka = baza.DatumIsteka,
                        OsnovnaPremija = baza.OsnovnaPremija, Valuta = baza.Valuta,
                        NacinPlacanja = baza.NacinPlacanja, Status = baza.Status,
                        UgovaracId = baza.UgovaracId, AgentId = baza.AgentId,
                        NazivSpecijalizacije = txtNazivSpecijalizacije.Text.Trim(),
                        OpisUslova = txtOpisUslova.Text.Trim()
                    };
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.dodajSpecijalizovanoOsiguranje(dtoSpec));
                    break;

                case "POLJOPRIVREDNO":
                    var dtoPolj = new PoljoprivrednoPregled
                    {
                        BrojPolise = baza.BrojPolise, TipOsiguranja = baza.TipOsiguranja,
                        DatumPocetka = baza.DatumPocetka, DatumIsteka = baza.DatumIsteka,
                        OsnovnaPremija = baza.OsnovnaPremija, Valuta = baza.Valuta,
                        NacinPlacanja = baza.NacinPlacanja, Status = baza.Status,
                        UgovaracId = baza.UgovaracId, AgentId = baza.AgentId
                    };
                    dtoPolj.UseviIds.AddRange(lstUsevi.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    dtoPolj.ZivotinjeIds.AddRange(lstZivotinje.SelectedItems.Cast<ComboItem>().Select(c => c.Id));
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.dodajPoljoprivrednoOsiguranje(dtoPolj));
                    break;

                default:
                    // Ostalih 5 tipova (ZIVOTNO/ZDRAVSTVENO/IMOVINSKO/AUTO/PUTNO) - nepromenjeno
                    // ponašanje kao i do sad: kreira se bazni zapis polise.
                    var dtoBaza = new PolisaBasic
                    {
                        BrojPolise = baza.BrojPolise, TipOsiguranja = baza.TipOsiguranja,
                        DatumPocetka = baza.DatumPocetka, DatumIsteka = baza.DatumIsteka,
                        OsnovnaPremija = baza.OsnovnaPremija, Valuta = baza.Valuta,
                        NacinPlacanja = baza.NacinPlacanja, Status = baza.Status,
                        UgovaracId = baza.UgovaracId, AgentId = baza.AgentId
                    };
                    uspeh = UiHelper.PokusajAkciju(() => DTOManager.dodajPolisu(dtoBaza));
                    break;
            }

            if (!uspeh) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
