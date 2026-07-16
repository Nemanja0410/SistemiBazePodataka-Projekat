using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class IzmeniKlijentaForma : Form
    {
        private readonly KlijentPregled _klijent;

        // Kontakt osobe - samo za PravnoLice/JavnaInstitucija (isto kao web klijent).
        private bool _imaKontakte => _klijent.TipKlijenta is "PRAVNO_LICE" or "JAVNA_INSTITUCIJA";

        public IzmeniKlijentaForma(KlijentPregled k)
        {
            _klijent = k;
            InitializeComponent();
            PopuniFormu();
        }

        private void DodajKontaktOsobeSekciju()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 8) };

            var lbl = new Label { Text = "Kontakt osobe", Font = new Font("Segoe UI Semibold", 9.5f), Dock = DockStyle.Top, Height = 24 };

            dgvKontakti = new DataGridView { Dock = DockStyle.Top, Height = 130 };
            UiHelper.StilizirajGrid(dgvKontakti);
            dgvKontakti.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false });
            dgvKontakti.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ime i prezime", FillWeight = 35 });
            dgvKontakti.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Funkcija", FillWeight = 25 });
            dgvKontakti.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Telefon", FillWeight = 20 });
            dgvKontakti.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", FillWeight = 20 });

            var tblUnos = new TableLayoutPanel { Dock = DockStyle.Top, Height = 60, ColumnCount = 5 };
            for (int i = 0; i < 4; i++) tblUnos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            tblUnos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            tblUnos.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            tblUnos.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            tblUnos.Controls.Add(new Label { Text = "Ime", AutoSize = true }, 0, 0);
            tblUnos.Controls.Add(new Label { Text = "Prezime", AutoSize = true }, 1, 0);
            tblUnos.Controls.Add(new Label { Text = "Funkcija", AutoSize = true }, 2, 0);
            tblUnos.Controls.Add(new Label { Text = "Telefon / Email", AutoSize = true }, 3, 0);
            txtKoIme = new TextBox { Dock = DockStyle.Fill };
            var txtKoPrezimeLocal = new TextBox { Dock = DockStyle.Fill };
            txtKoPrezime = txtKoPrezimeLocal;
            txtKoFunkcija = new TextBox { Dock = DockStyle.Fill };
            txtKoTelefon = new TextBox { Dock = DockStyle.Fill };
            tblUnos.Controls.Add(txtKoIme, 0, 1);
            tblUnos.Controls.Add(txtKoPrezime, 1, 1);
            tblUnos.Controls.Add(txtKoFunkcija, 2, 1);
            tblUnos.Controls.Add(txtKoTelefon, 3, 1);
            var btnDodajKontakt = UiHelper.NapraviDugme("➕ Dodaj", UiHelper.Zelena, 76);
            btnDodajKontakt.Dock = DockStyle.Fill;
            tblUnos.Controls.Add(btnDodajKontakt, 4, 1);
            txtKoEmail = new TextBox();
            var btnUkloni = UiHelper.NapraviDugme("🗑️ Ukloni izabrani kontakt", UiHelper.Crvena, 190);

            var pnlDrugiRed = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 30, FlowDirection = FlowDirection.LeftToRight };
            pnlDrugiRed.Controls.Add(new Label { Text = "Email:", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
            txtKoEmail.Width = 220;
            pnlDrugiRed.Controls.Add(txtKoEmail);
            pnlDrugiRed.Controls.Add(btnUkloni);

            btnDodajKontakt.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtKoIme.Text) || string.IsNullOrWhiteSpace(txtKoPrezime.Text))
                { MessageBox.Show("Ime i prezime su obavezni.", "Validacija"); return; }
                var dto = new KontaktOsobaBasic
                {
                    KlijentId = _klijent.KlijentId,
                    Ime = txtKoIme.Text.Trim(), Prezime = txtKoPrezime.Text.Trim(),
                    Funkcija = txtKoFunkcija.Text.Trim(), Telefon = txtKoTelefon.Text.Trim(),
                    Email = txtKoEmail.Text.Trim()
                };
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajKontakt(dto))) return;
                txtKoIme.Clear(); txtKoPrezime.Clear(); txtKoFunkcija.Clear(); txtKoTelefon.Clear(); txtKoEmail.Clear();
                UcitajKontakte();
            };

            btnUkloni.Click += (s, e) =>
            {
                if (dgvKontakti.CurrentRow?.Tag is not KontaktOsobaBasic ko)
                { MessageBox.Show("Izaberite kontakt za uklanjanje.", "Info"); return; }
                if (MessageBox.Show($"Ukloniti kontakt \"{ko.Ime} {ko.Prezime}\"?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiKontakt(ko.KontaktId))) return;
                UcitajKontakte();
            };

            pnl.Controls.Add(dgvKontakti);
            pnl.Controls.Add(pnlDrugiRed);
            pnl.Controls.Add(tblUnos);
            pnl.Controls.Add(lbl);
            this.Controls.Add(pnl);

            UcitajKontakte();
        }

        private void UcitajKontakte()
        {
            dgvKontakti.Rows.Clear();
            foreach (var ko in DTOManager.vratiKontakteZaKlijenta(_klijent.KlijentId))
            {
                int r = dgvKontakti.Rows.Add(ko.KontaktId, $"{ko.Ime} {ko.Prezime}", ko.Funkcija, ko.Telefon, ko.Email);
                dgvKontakti.Rows[r].Tag = ko;
            }
        }

        private void PopuniFormu()
        {
            txtNaziv.Text          = _klijent.Naziv ?? "";
            txtAdresa.Text         = _klijent.Adresa ?? "";
            txtTelefon.Text        = _klijent.Telefon ?? "";
            txtEmail.Text          = _klijent.Email ?? "";
            cmbStatus.SelectedItem = _klijent.Status ?? "AKTIVAN";
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNaziv.Text))
            { MessageBox.Show("Naziv je obavezan.", "Validacija"); txtNaziv.Focus(); return; }

            var dto = new KlijentBasic
            {
                KlijentId = _klijent.KlijentId,
                Naziv     = txtNaziv.Text.Trim(),
                Adresa    = txtAdresa.Text.Trim(),
                Telefon   = txtTelefon.Text.Trim(),
                Email     = txtEmail.Text.Trim(),
                Status    = cmbStatus.SelectedItem?.ToString()
            };

            if (!UiHelper.PokusajAkciju(() => DTOManager.azurirajKlijenta(dto))) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
