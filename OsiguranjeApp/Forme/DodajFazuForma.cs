using System;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class DodajFazuForma : Form
    {
        private readonly int _stetaId;
        private readonly int _redniBroj;
        private readonly FazaObradeBasic? _postojeca;

        public DodajFazuForma(int stetaId, int redniBroj)
        {
            _stetaId   = stetaId;
            _redniBroj = redniBroj;
            InitializeComponent();
            UcitajOsoblje();
        }

        public DodajFazuForma(FazaObradeBasic postojeca)
        {
            _postojeca = postojeca;
            _stetaId   = postojeca.StetaId;
            _redniBroj = postojeca.RedniBrojFaze;
            InitializeComponent();
            UcitajOsoblje();
            PopuniFormu(postojeca);
        }

        private void UcitajOsoblje()
        {
            cmbOdgovornoLice.Items.Add(new ComboItem(0, "-- Bez odgovornog lica --"));
            foreach (var o in DTOManager.vratiSveOsoblje())
                cmbOdgovornoLice.Items.Add(new ComboItem(o.OsobljeId, $"{o.Ime} {o.Prezime}"));
            cmbOdgovornoLice.SelectedIndex = 0;
        }

        private void PopuniFormu(FazaObradeBasic f)
        {
            txtNaziv.Text        = f.NazivFaze;
            dtpPocetka.Value     = f.DatumPocetka;
            chkZavrsena.Checked  = f.DatumZavrsetka.HasValue;
            dtpZavrsetka.Enabled = f.DatumZavrsetka.HasValue;
            if (f.DatumZavrsetka.HasValue) dtpZavrsetka.Value = f.DatumZavrsetka.Value;

            for (int i = 0; i < cmbOdgovornoLice.Items.Count; i++)
                if ((cmbOdgovornoLice.Items[i] as ComboItem)?.Id == (f.OdgovornoLiceId ?? 0))
                { cmbOdgovornoLice.SelectedIndex = i; break; }

            if (!string.IsNullOrEmpty(f.Odluka)) cmbOdluka.SelectedItem = f.Odluka;
            txtDokumentacija.Text = f.Dokumentacija;
            txtNapomena.Text      = f.Napomena;
        }

        private void BtnSacuvaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNaziv.Text))
            { MessageBox.Show("Naziv faze je obavezan.", "Validacija"); txtNaziv.Focus(); return; }

            var ol = (ComboItem)cmbOdgovornoLice.SelectedItem!;

            var dto = new FazaObradeBasic
            {
                FazaId           = _postojeca?.FazaId ?? 0,
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

            bool uspeh = _postojeca == null
                ? UiHelper.PokusajAkciju(() => DTOManager.dodajFazuObrade(dto))
                : UiHelper.PokusajAkciju(() => DTOManager.azurirajFazuObrade(dto));
            if (!uspeh) return;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
