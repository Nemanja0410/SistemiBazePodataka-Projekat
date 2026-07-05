using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class IzmeniPolisaForma : Form
    {
        private readonly PolisaBasic _polisa;
        private TextBox       txtPremija = null!;
        private ComboBox      cmbStatus = null!, cmbNacin = null!, cmbUgovarac = null!, cmbAgent = null!;
        private DateTimePicker dtpPocetka = null!, dtpIsteka = null!;
        private Button        btnSacuvaj = null!, btnOdustani = null!;

        public IzmeniPolisaForma(PolisaBasic p)
        {
            _polisa = p;
            InitializeComponent();
            UcitajComboove();
            PopuniFormu();
        }

        private void InitializeComponent()
        {
            this.Text            = $"Izmeni polisu — {_polisa.BrojPolise}";
            this.Size            = new Size(470, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(8);

            cmbUgovarac = UiHelper.DodajComboRed(tbl, 0, "Ugovarač *:");
            cmbAgent    = UiHelper.DodajComboRed(tbl, 1, "Agent:");
            dtpPocetka  = UiHelper.DodajDTPRed(tbl, 2, "Datum početka *:");
            dtpIsteka   = UiHelper.DodajDTPRed(tbl, 3, "Datum isteka *:");
            txtPremija  = UiHelper.DodajRed(tbl, 4, "Osnovna premija *:");
            cmbNacin    = UiHelper.DodajComboRed(tbl, 5, "Način plaćanja:");
            cmbNacin.Items.AddRange(new[] { "JEDNOKRATNO","MESECNO","KVARTAL","POLUGODISNJE","GODISNJE" });
            cmbStatus   = UiHelper.DodajComboRed(tbl, 6, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVNA","ISTEKLA","RASKINUTA","MIROVANJE","OBNOVLJENA" });

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 7);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
        }

        private void UcitajComboove()
        {
            cmbUgovarac.Items.Add(new ComboItem(0, "-- Izaberi --"));
            foreach (var k in DTOManager.vratiSveKlijente())
                cmbUgovarac.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbUgovarac.SelectedIndex = 0;

            cmbAgent.Items.Add(new ComboItem(0, "-- Bez agenta --"));
            foreach (var a in DTOManager.vratiSveAgente())
                cmbAgent.Items.Add(new ComboItem(a.OsobljeId, $"{a.Ime} {a.Prezime}"));
            cmbAgent.SelectedIndex = 0;
        }

        private void PopuniFormu()
        {
            dtpPocetka.Value       = _polisa.DatumPocetka;
            dtpIsteka.Value        = _polisa.DatumIsteka;
            txtPremija.Text        = _polisa.OsnovnaPremija.ToString("F2");
            cmbNacin.SelectedItem  = _polisa.NacinPlacanja;
            cmbStatus.SelectedItem = _polisa.Status;

            for (int i = 0; i < cmbUgovarac.Items.Count; i++)
                if (((ComboItem)cmbUgovarac.Items[i]).Id == _polisa.UgovaracId)
                { cmbUgovarac.SelectedIndex = i; break; }

            if (_polisa.AgentId.HasValue)
                for (int i = 0; i < cmbAgent.Items.Count; i++)
                    if (((ComboItem)cmbAgent.Items[i]).Id == _polisa.AgentId.Value)
                    { cmbAgent.SelectedIndex = i; break; }
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (((ComboItem)cmbUgovarac.SelectedItem)?.Id == 0)
            { MessageBox.Show("Izaberite ugovarača.", "Validacija"); return; }
            if (!decimal.TryParse(txtPremija.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal prem) || prem <= 0)
            { MessageBox.Show("Premija mora biti pozitivan broj.", "Validacija"); return; }
            if (dtpIsteka.Value <= dtpPocetka.Value)
            { MessageBox.Show("Datum isteka mora biti posle početka.", "Validacija"); return; }

            var agent = (ComboItem)cmbAgent.SelectedItem;
            _polisa.DatumPocetka   = dtpPocetka.Value.Date;
            _polisa.DatumIsteka    = dtpIsteka.Value.Date;
            _polisa.OsnovnaPremija = prem;
            _polisa.NacinPlacanja  = cmbNacin.SelectedItem?.ToString();
            _polisa.Status         = cmbStatus.SelectedItem?.ToString();
            _polisa.UgovaracId     = ((ComboItem)cmbUgovarac.SelectedItem!).Id;
            _polisa.AgentId        = agent?.Id > 0 ? agent.Id : (int?)null;

            DTOManager.azurirajPolisu(_polisa);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
