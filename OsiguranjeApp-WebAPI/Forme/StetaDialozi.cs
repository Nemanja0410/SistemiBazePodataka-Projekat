using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class DodajSteteForma : Form
    {
        private TextBox       txtBroj = null!, txtOpis = null!, txtLokacija = null!, txtIznos = null!;
        private ComboBox      cmbVrsta = null!, cmbStatus = null!, cmbPolisa = null!, cmbPodnosilac = null!;
        private DateTimePicker dtpNastanka = null!;
        private Button        btnSacuvaj = null!, btnOdustani = null!;
        private List<PolisaPregled> _svePolise = new();

        public DodajSteteForma()
        {
            InitializeComponent();
            UcitajComboove();
        }

        private void InitializeComponent()
        {
            this.Text= "Nova šteta";
            this.Size= new Size(490, 470);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox= false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font= new Font("Segoe UI", 9f);
            this.BackColor= Color.White;

            var tbl = UiHelper.NapraviLayout(10);

            cmbVrsta = UiHelper.DodajComboRed(tbl, 0, "Vrsta štete *:");
            cmbVrsta.Items.AddRange(new[] { "AUTO","ZDRAVSTVENA","IMOVINSKA","PUTNA","ZIVOTNA","OSTALO" });
            cmbVrsta.SelectedIndex = 0;
            cmbVrsta.SelectedIndexChanged += (s, e) => txtBroj.Text = GenerisiBroj();

            txtBroj= UiHelper.DodajRed(tbl, 1, "Broj štete *:");
            cmbPodnosilac = UiHelper.DodajComboRed(tbl, 2, "Podnosilac *:");
            cmbPolisa= UiHelper.DodajComboRed(tbl, 3, "Polisa *:");
            cmbPodnosilac.SelectedIndexChanged += CmbPodnosilac_SelectedIndexChanged;
            dtpNastanka   = UiHelper.DodajDTPRed(tbl, 4, "Datum nastanka *:");
            txtLokacija   = UiHelper.DodajRed(tbl, 5, "Lokacija:");
            txtOpis= UiHelper.DodajRed(tbl, 6, "Opis događaja:");
            txtIznos= UiHelper.DodajRed(tbl, 7, "Procenjeni iznos:");
            cmbStatus= UiHelper.DodajComboRed(tbl, 8, "Status:");
            cmbStatus.Items.AddRange(new[] { "PRIJAVLJENA","U_OBRADI","U_PROCENI","ODOBRENA","ODBIJENA","ISPLACENA","ZATVORENA" });
            cmbStatus.SelectedIndex = 0;

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 9);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
        }

        private string GenerisiBroj()
        {
            var v    = cmbVrsta.SelectedItem?.ToString() ?? "ST";
            var pref = v.Length >= 3 ? v.Substring(0, 3) : v;
            return $"STE-{pref}-{DateTime.Now.Year}-{new Random().Next(100, 999)}";
        }

        private void UcitajComboove()
        {
            _svePolise = DTOManager.vratiSvePolise();

            cmbPodnosilac.Items.Add(new ComboItem(0, "-- Izaberi klijenta --"));
            foreach (var k in DTOManager.vratiSveKlijente())
                cmbPodnosilac.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbPodnosilac.SelectedIndex = 0;

            OsveziPolise(0);

            txtBroj.Text = GenerisiBroj();
        }

        private void CmbPodnosilac_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int klijentId = (cmbPodnosilac.SelectedItem as ComboItem)?.Id ?? 0;
            OsveziPolise(klijentId);
        }

        private void OsveziPolise(int klijentId)
        {
            cmbPolisa.Items.Clear();
            cmbPolisa.Items.Add(new ComboItem(0, "-- Izaberi polisu --"));
            foreach (var p in _svePolise.Where(p => p.UgovaracId == klijentId))
                cmbPolisa.Items.Add(new ComboItem(p.PolisaId, $"{p.BrojPolise} ({p.TipOsiguranja})"));
            cmbPolisa.SelectedIndex = 0;
            cmbPolisa.Enabled = klijentId != 0;
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if ((cmbPolisa.SelectedItem as ComboItem)?.Id == 0)
            { MessageBox.Show("Izaberite polisu.", "Validacija"); return; }
            if ((cmbPodnosilac.SelectedItem as ComboItem)?.Id == 0)
            { MessageBox.Show("Izaberite podnosioca.", "Validacija"); return; }

            decimal? iznos = null;
            if (!string.IsNullOrWhiteSpace(txtIznos.Text))
                if (decimal.TryParse(txtIznos.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal iz))
                    iznos = iz;

            var dto = new StetaBasic
            {
                BrojStete= txtBroj.Text.Trim(),
                VrstaStete= cmbVrsta.SelectedItem?.ToString(),
                PolisaId= ((ComboItem)cmbPolisa.SelectedItem!).Id,
                PodnosilacId   = ((ComboItem)cmbPodnosilac.SelectedItem!).Id,
                DatumNastanka  = dtpNastanka.Value.Date,
                Lokacija= txtLokacija.Text.Trim(),
                OpisDogodjaja  = txtOpis.Text.Trim(),
                ProcenjeniIznos = iznos,
                Status= cmbStatus.SelectedItem?.ToString()
            };

            if (!UiHelper.PokusajAkciju(() => DTOManager.dodajStetu(dto))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public class IzmeniSteteForma : Form
    {
        private readonly StetaBasic _steta;
        private TextBox       txtOpis = null!, txtLokacija = null!, txtIznos = null!;
        private ComboBox      cmbStatus = null!;
        private DateTimePicker dtpNastanka = null!;
        private Button        btnSacuvaj = null!, btnOdustani = null!;

        public IzmeniSteteForma(StetaBasic st)
        {
            _steta = st;
            InitializeComponent();
            PopuniFormu();
        }

        private void InitializeComponent()
        {
            this.Text= $"Izmeni štetu — {_steta.BrojStete}";
            this.Size= new Size(460, 360);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font= new Font("Segoe UI", 9f);
            this.BackColor= Color.White;

            var tbl = UiHelper.NapraviLayout(7);

            dtpNastanka = UiHelper.DodajDTPRed(tbl, 0, "Datum nastanka *:");
            txtLokacija = UiHelper.DodajRed(tbl, 1, "Lokacija:");
            txtOpis= UiHelper.DodajRed(tbl, 2, "Opis događaja:");
            txtIznos    = UiHelper.DodajRed(tbl, 3, "Procenjeni iznos:");
            cmbStatus= UiHelper.DodajComboRed(tbl, 4, "Status:");
            cmbStatus.Items.AddRange(new[] { "PRIJAVLJENA","U_OBRADI","U_PROCENI","ODOBRENA","ODBIJENA","ISPLACENA","ZATVORENA" });

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 5);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
        }

        private void PopuniFormu()
        {
            dtpNastanka.Value      = _steta.DatumNastanka;
            txtLokacija.Text= _steta.Lokacija ?? "";
            txtOpis.Text= _steta.OpisDogodjaja ?? "";
            txtIznos.Text= _steta.ProcenjeniIznos?.ToString("F2") ?? "";
            cmbStatus.SelectedItem = _steta.Status;
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            decimal? iznos = null;
            if (!string.IsNullOrWhiteSpace(txtIznos.Text))
                if (decimal.TryParse(txtIznos.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal iz))
                    iznos = iz;

            _steta.DatumNastanka  = dtpNastanka.Value.Date;
            _steta.Lokacija= txtLokacija.Text.Trim();
            _steta.OpisDogodjaja  = txtOpis.Text.Trim();
            _steta.ProcenjeniIznos = iznos;
            _steta.Status= cmbStatus.SelectedItem?.ToString();

            if (!UiHelper.PokusajAkciju(() => DTOManager.azurirajStetu(_steta))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
