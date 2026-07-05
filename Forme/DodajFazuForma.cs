using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public class DodajFazuForma : Form
    {
        private readonly int _stetaId;
        private readonly int _redniBroj;

        private TextBox       txtNaziv = null!, txtDokumentacija = null!, txtNapomena = null!;
        private ComboBox      cmbOdluka = null!, cmbOdgovornoLice = null!;
        private DateTimePicker dtpPocetka = null!, dtpZavrsetka = null!;
        private CheckBox      chkZavrsena = null!;
        private Button        btnSacuvaj = null!, btnOdustani = null!;

        public DodajFazuForma(int stetaId, int redniBroj)
        {
            _stetaId   = stetaId;
            _redniBroj = redniBroj;
            InitializeComponent();
            UcitajOsoblje();
        }

        private void InitializeComponent()
        {
            this.Text            = $"Nova faza obrade (br. {_redniBroj})";
            this.Size            = new Size(470, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            var tbl = UiHelper.NapraviLayout(9);

            txtNaziv   = UiHelper.DodajRed(tbl, 0, "Naziv faze *:");
            dtpPocetka = UiHelper.DodajDTPRed(tbl, 1, "Datum početka *:");

            // CheckBox za završenost — zauzima obe kolone
            chkZavrsena = new CheckBox
            {
                Text = "Faza je završena",
                Dock = DockStyle.Fill
            };
            chkZavrsena.CheckedChanged += (s, e) => dtpZavrsetka.Enabled = chkZavrsena.Checked;
            tbl.Controls.Add(chkZavrsena, 0, 2);
            tbl.SetColumnSpan(chkZavrsena, 2);

            dtpZavrsetka = UiHelper.DodajDTPRed(tbl, 3, "Datum završetka:");
            dtpZavrsetka.Enabled = false;

            cmbOdgovornoLice  = UiHelper.DodajComboRed(tbl, 4, "Odgovorno lice:");
            cmbOdluka         = UiHelper.DodajComboRed(tbl, 5, "Odluka:");
            cmbOdluka.Items.AddRange(new[] { "U_TOKU", "ODOBRENA", "ODBIJENA", "POTREBNA_DOPUNA" });
            cmbOdluka.SelectedIndex = 0;

            txtDokumentacija = UiHelper.DodajRed(tbl, 6, "Dokumentacija:");
            txtNapomena      = UiHelper.DodajRed(tbl, 7, "Napomena:");

            var (btnOk, btnCancel) = UiHelper.DodajDugmadPanel(tbl, 8);
            btnSacuvaj  = btnOk;
            btnOdustani = btnCancel;
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(tbl);
        }

        private void UcitajOsoblje()
        {
            cmbOdgovornoLice.Items.Add(new ComboItem(0, "-- Bez odgovornog lica --"));
            foreach (var o in DTOManager.vratiSveOsoblje())
                cmbOdgovornoLice.Items.Add(new ComboItem(o.OsobljeId, $"{o.Ime} {o.Prezime}"));
            cmbOdgovornoLice.SelectedIndex = 0;
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNaziv.Text))
            { MessageBox.Show("Naziv faze je obavezan.", "Validacija"); txtNaziv.Focus(); return; }

            var ol = (ComboItem)cmbOdgovornoLice.SelectedItem;

            var dto = new FazaObradeBasic
            {
                StetaId          = _stetaId,
                RedniBrojFaze    = _redniBroj,
                NazivFaze        = txtNaziv.Text.Trim(),
                DatumPocetka     = dtpPocetka.Value.Date,
                DatumZavrsetka   = chkZavrsena.Checked ? dtpZavrsetka.Value.Date : (DateTime?)null,
                OdgovornoLiceId  = ol?.Id > 0 ? ol.Id : (int?)null,
                Odluka           = cmbOdluka.SelectedItem?.ToString(),
                Dokumentacija    = txtDokumentacija.Text.Trim(),
                Napomena         = txtNapomena.Text.Trim()
            };

            DTOManager.dodajFazuObrade(dto);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
