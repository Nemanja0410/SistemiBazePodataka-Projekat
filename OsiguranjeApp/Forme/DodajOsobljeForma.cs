using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class DodajOsobljeForma : Form
    {
        public DodajOsobljeForma()
        {
            InitializeComponent();
        }

        private void CmbTip_SelectedIndexChanged(object? sender, EventArgs e) => AzurirajVidljivostPoTipu();

        private void AzurirajVidljivostPoTipu()
        {
            bool jePravnik = cmbTip.SelectedItem?.ToString() == "PRAVNIK";
            bool jeProcenitelj = cmbTip.SelectedItem?.ToString() == "PROCENITELJ";
            bool jeLekar = cmbTip.SelectedItem?.ToString() == "LEKAR";

            cmbTipPravnika.Visible = jePravnik;
            txtBarBroj.Visible     = jePravnik;
            tbl.GetControlFromPosition(0, 8)!.Visible = jePravnik;
            tbl.GetControlFromPosition(0, 9)!.Visible = jePravnik;
            tbl.RowStyles[8].Height = jePravnik ? 36 : 0;
            tbl.RowStyles[9].Height = jePravnik ? 36 : 0;

            // Broj licence polje je zajednicko za Procenitelja i Lekara (isti koncept).
            bool imaLicencu = jeProcenitelj || jeLekar;
            txtBrojLicence.Visible = imaLicencu;
            tbl.GetControlFromPosition(0, 10)!.Visible = imaLicencu;
            tbl.RowStyles[10].Height = imaLicencu ? 36 : 0;

            txtSpecijalizacija.Visible = jeLekar;
            tbl.GetControlFromPosition(0, 11)!.Visible = jeLekar;
            tbl.RowStyles[11].Height = jeLekar ? 36 : 0;

            int sadrzajVisina = 0;
            foreach (RowStyle rs in tbl.RowStyles) sadrzajVisina += (int)rs.Height;
            this.ClientSize = new Size(450, sadrzajVisina + tbl.Padding.Top + tbl.Padding.Bottom);
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIme.Text))
            { MessageBox.Show("Ime je obavezno.", "Validacija"); txtIme.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtPrezime.Text))
            { MessageBox.Show("Prezime je obavezno.", "Validacija"); txtPrezime.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtJmbg.Text) || txtJmbg.Text.Length != 13)
            { MessageBox.Show("JMBG mora imati 13 cifara.", "Validacija"); txtJmbg.Focus(); return; }

            bool jePravnik = cmbTip.SelectedItem?.ToString() == "PRAVNIK";
            bool jeProcenitelj = cmbTip.SelectedItem?.ToString() == "PROCENITELJ";
            bool jeLekar = cmbTip.SelectedItem?.ToString() == "LEKAR";
            if (jePravnik && cmbTipPravnika.SelectedItem == null)
            { MessageBox.Show("Tip pravnika je obavezan.", "Validacija"); cmbTipPravnika.Focus(); return; }

            bool uspeh;
            if (jeLekar)
            {
                var dtoLekar = new LekarBasic
                {
                    Ime = txtIme.Text.Trim(), Prezime = txtPrezime.Text.Trim(),
                    Jmbg = txtJmbg.Text.Trim(), Adresa = txtAdresa.Text.Trim(),
                    Telefon = txtTelefon.Text.Trim(), Email = txtEmail.Text.Trim(),
                    Status = cmbStatus.SelectedItem?.ToString(),
                    Specijalizacija = txtSpecijalizacija.Text.Trim(),
                    LicencaBroj = txtBrojLicence.Text.Trim()
                };
                uspeh = UiHelper.PokusajAkciju(() => DTOManager.dodajLekara(dtoLekar));
            }
            else
            {
                var dto = new OsobljeBasic
                {
                    TipOsoblja  = cmbTip.SelectedItem?.ToString(),
                    Ime         = txtIme.Text.Trim(),
                    Prezime     = txtPrezime.Text.Trim(),
                    Jmbg        = txtJmbg.Text.Trim(),
                    Adresa      = txtAdresa.Text.Trim(),
                    Telefon     = txtTelefon.Text.Trim(),
                    Email       = txtEmail.Text.Trim(),
                    Status      = cmbStatus.SelectedItem?.ToString(),
                    TipPravnika = jePravnik ? cmbTipPravnika.SelectedItem?.ToString() : null,
                    BarBroj     = jePravnik ? txtBarBroj.Text.Trim() : null,
                    BrojLicence = jeProcenitelj ? txtBrojLicence.Text.Trim() : null
                };
                uspeh = UiHelper.PokusajAkciju(() => DTOManager.dodajOsoblje(dto));
            }

            if (!uspeh) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
