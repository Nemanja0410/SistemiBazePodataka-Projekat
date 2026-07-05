using System;
using System.Drawing;
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

        public DodajPolisaForma()
        {
            InitializeComponent();
            UcitajComboove();
        }

        private void InitializeComponent()
        {
            this.Text            = "Nova polisa osiguranja";
            this.Size            = new Size(490, 460);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(11);

            cmbTip     = UiHelper.DodajComboRed(tbl, 0, "Tip osiguranja *:");
            cmbTip.Items.AddRange(new[] { "ZIVOTNO","ZDRAVSTVENO","IMOVINSKO","AUTO","PUTNO","POLJOPRIVREDNO","ODGOVORNOST","SPECIJALIZOVANO" });
            cmbTip.SelectedIndex = 0;
            cmbTip.SelectedIndexChanged += (s, e) => txtBroj.Text = GenerisiBroj();

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

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 10);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
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

            txtBroj.Text = GenerisiBroj();
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBroj.Text))
            { MessageBox.Show("Broj polise je obavezan.", "Validacija"); return; }
            if (((ComboItem)cmbUgovarac.SelectedItem)?.Id == 0)
            { MessageBox.Show("Izaberite ugovarača.", "Validacija"); return; }
            if (!decimal.TryParse(txtPremija.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal prem) || prem <= 0)
            { MessageBox.Show("Premija mora biti pozitivan broj.", "Validacija"); return; }
            if (dtpIsteka.Value <= dtpPocetka.Value)
            { MessageBox.Show("Datum isteka mora biti posle datuma početka.", "Validacija"); return; }

            var agent = (ComboItem)cmbAgent.SelectedItem;
            var dto = new PolisaBasic
            {
                BrojPolise    = txtBroj.Text.Trim(),
                TipOsiguranja = cmbTip.SelectedItem?.ToString(),
                DatumPocetka  = dtpPocetka.Value.Date,
                DatumIsteka   = dtpIsteka.Value.Date,
                OsnovnaPremija = prem,
                Valuta        = cmbValuta.SelectedItem?.ToString(),
                NacinPlacanja = cmbNacin.SelectedItem?.ToString(),
                Status        = cmbStatus.SelectedItem?.ToString(),
                UgovaracId    = ((ComboItem)cmbUgovarac.SelectedItem!).Id,
                AgentId       = agent?.Id > 0 ? agent.Id : (int?)null
            };

            DTOManager.dodajPolisu(dto);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
