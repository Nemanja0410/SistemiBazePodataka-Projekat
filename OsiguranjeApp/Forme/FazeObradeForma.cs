using System;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    public partial class FazeObradeForma : Form
    {
        private readonly StetaBasic _steta;

        public FazeObradeForma(StetaBasic st)
        {
            _steta = st;
            InitializeComponent();
            UcitajFaze();
        }

        private void UcitajFaze()
        {
            dgvFaze.Rows.Clear();
            foreach (var f in _steta.FazeObrade)
            {
                string zav = f.DatumZavrsetka.HasValue
                    ? f.DatumZavrsetka.Value.ToString("dd.MM.yyyy") : "—";

                int r = dgvFaze.Rows.Add(
                    f.RedniBrojFaze,
                    f.NazivFaze,
                    f.DatumPocetka.ToString("dd.MM.yyyy"),
                    zav,
                    f.OdgovornoLiceIme ?? "/",
                    f.Odluka ?? "/",
                    f.Dokumentacija ?? "/");

                if (!string.IsNullOrEmpty(f.Odluka))
                    dgvFaze.Rows[r].Cells[5].Style.ForeColor = UiHelper.StatusBoja(f.Odluka);

                dgvFaze.Rows[r].Tag = f;
            }
            btnIzmeniFazu.Enabled = false;
        }

        private FazaObradeBasic? odabranaFaza() =>
            dgvFaze.CurrentRow?.Tag as FazaObradeBasic;

        private void BtnDodajFazu_Click(object? sender, EventArgs e)
        {
            var f = new DodajFazuForma(_steta.StetaId, _steta.FazeObrade.Count + 1);
            if (f.ShowDialog() == DialogResult.OK) OsveziFaze();
        }

        private void BtnIzmeniFazu_Click(object? sender, EventArgs e)
        {
            var faza = odabranaFaza();
            if (faza == null) { MessageBox.Show("Izaberite fazu.", "Info"); return; }
            var f = new DodajFazuForma(faza);
            if (f.ShowDialog() == DialogResult.OK) OsveziFaze();
        }

        private void OsveziFaze()
        {
            var osvezen = DTOManager.vratiStetu(_steta.StetaId);
            _steta.FazeObrade.Clear();
            foreach (var faza in osvezen.FazeObrade)
                _steta.FazeObrade.Add(faza);
            UcitajFaze();
        }
    }
}
