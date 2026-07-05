using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // ================================================================
    //  DODAJ STETU
    // ================================================================
    public class DodajSteteForma : Form
    {
        private TextBox       txtBroj = null!, txtOpis = null!, txtLokacija = null!, txtIznos = null!;
        private ComboBox      cmbVrsta = null!, cmbStatus = null!, cmbPolisa = null!, cmbPodnosilac = null!;
        private DateTimePicker dtpNastanka = null!;
        private Button        btnSacuvaj = null!, btnOdustani = null!;

        public DodajSteteForma()
        {
            InitializeComponent();
            UcitajComboove();
        }

        private void InitializeComponent()
        {
            this.Text            = "Nova šteta";
            this.Size            = new Size(490, 440);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(10);

            cmbVrsta = UiHelper.DodajComboRed(tbl, 0, "Vrsta štete *:");
            cmbVrsta.Items.AddRange(new[] { "AUTO","ZDRAVSTVENA","IMOVINSKA","PUTNA","ZIVOTNA","OSTALO" });
            cmbVrsta.SelectedIndex = 0;
            cmbVrsta.SelectedIndexChanged += (s, e) => txtBroj.Text = GenerisiBroj();

            txtBroj       = UiHelper.DodajRed(tbl, 1, "Broj štete *:");
            cmbPolisa     = UiHelper.DodajComboRed(tbl, 2, "Polisa *:");
            cmbPodnosilac = UiHelper.DodajComboRed(tbl, 3, "Podnosilac *:");
            dtpNastanka   = UiHelper.DodajDTPRed(tbl, 4, "Datum nastanka *:");
            txtLokacija   = UiHelper.DodajRed(tbl, 5, "Lokacija:");
            txtOpis       = UiHelper.DodajRed(tbl, 6, "Opis događaja:");
            txtIznos      = UiHelper.DodajRed(tbl, 7, "Procenjeni iznos:");
            cmbStatus     = UiHelper.DodajComboRed(tbl, 8, "Status:");
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
            cmbPolisa.Items.Add(new ComboItem(0, "-- Izaberi polisu --"));
            foreach (var p in DTOManager.vratiSvePolise())
                cmbPolisa.Items.Add(new ComboItem(p.PolisaId, $"{p.BrojPolise} ({p.TipOsiguranja})"));
            cmbPolisa.SelectedIndex = 0;

            cmbPodnosilac.Items.Add(new ComboItem(0, "-- Izaberi klijenta --"));
            foreach (var k in DTOManager.vratiSveKlijente())
                cmbPodnosilac.Items.Add(new ComboItem(k.KlijentId, k.Naziv));
            cmbPodnosilac.SelectedIndex = 0;

            txtBroj.Text = GenerisiBroj();
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (((ComboItem)cmbPolisa.SelectedItem)?.Id == 0)
            { MessageBox.Show("Izaberite polisu.", "Validacija"); return; }
            if (((ComboItem)cmbPodnosilac.SelectedItem)?.Id == 0)
            { MessageBox.Show("Izaberite podnosioca.", "Validacija"); return; }

            decimal? iznos = null;
            if (!string.IsNullOrWhiteSpace(txtIznos.Text))
                if (decimal.TryParse(txtIznos.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal iz))
                    iznos = iz;

            var dto = new StetaBasic
            {
                BrojStete      = txtBroj.Text.Trim(),
                VrstaStete     = cmbVrsta.SelectedItem?.ToString(),
                PolisaId       = ((ComboItem)cmbPolisa.SelectedItem!).Id,
                PodnosilacId   = ((ComboItem)cmbPodnosilac.SelectedItem!).Id,
                DatumNastanka  = dtpNastanka.Value.Date,
                Lokacija       = txtLokacija.Text.Trim(),
                OpisDogodjaja  = txtOpis.Text.Trim(),
                ProcenjeniIznos = iznos,
                Status         = cmbStatus.SelectedItem?.ToString()
            };

            DTOManager.dodajStetu(dto);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // ================================================================
    //  IZMENI STETU
    // ================================================================
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
            this.Text            = $"Izmeni štetu — {_steta.BrojStete}";
            this.Size            = new Size(460, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(7);

            dtpNastanka = UiHelper.DodajDTPRed(tbl, 0, "Datum nastanka *:");
            txtLokacija = UiHelper.DodajRed(tbl, 1, "Lokacija:");
            txtOpis     = UiHelper.DodajRed(tbl, 2, "Opis događaja:");
            txtIznos    = UiHelper.DodajRed(tbl, 3, "Procenjeni iznos:");
            cmbStatus   = UiHelper.DodajComboRed(tbl, 4, "Status:");
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
            txtLokacija.Text       = _steta.Lokacija ?? "";
            txtOpis.Text           = _steta.OpisDogodjaja ?? "";
            txtIznos.Text          = _steta.ProcenjeniIznos?.ToString("F2") ?? "";
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
            _steta.Lokacija       = txtLokacija.Text.Trim();
            _steta.OpisDogodjaja  = txtOpis.Text.Trim();
            _steta.ProcenjeniIznos = iznos;
            _steta.Status         = cmbStatus.SelectedItem?.ToString();

            DTOManager.azurirajStetu(_steta);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
