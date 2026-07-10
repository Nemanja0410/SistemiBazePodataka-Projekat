using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class IzmeniKlijentaForma : Form
    {
        private readonly KlijentPregled _klijent;
        private TextBox  txtNaziv = null!, txtAdresa = null!, txtTelefon = null!, txtEmail = null!;
        private ComboBox cmbStatus = null!;
        private Button   btnSacuvaj = null!, btnOdustani = null!;

        public IzmeniKlijentaForma(KlijentPregled k)
        {
            _klijent = k;
            InitializeComponent();
            PopuniFormu();
        }

        private void InitializeComponent()
        {
            this.Text            = $"Izmeni klijenta — {_klijent.Naziv}";
            this.Size            = new Size(450, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(6);

            txtNaziv   = UiHelper.DodajRed(tbl, 0, "Naziv / Ime *:");
            txtAdresa  = UiHelper.DodajRed(tbl, 1, "Adresa:");
            txtTelefon = UiHelper.DodajRed(tbl, 2, "Telefon:");
            txtEmail   = UiHelper.DodajRed(tbl, 3, "Email:");
            cmbStatus  = UiHelper.DodajComboRed(tbl, 4, "Status:");
            cmbStatus.Items.AddRange(new[] { "AKTIVAN", "NEAKTIVAN", "BLOKIRAN" });

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 5);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
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
