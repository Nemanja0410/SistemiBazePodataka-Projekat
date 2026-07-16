using System;
using System.Linq;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // Oblasti procene kojima se bavi konkretan Procenitelj - fiksan skup (Vozilo/Imovina/
    // Zdravstvo/Specijalne štete), bira se iz padajuce liste, isto nacelo kao web klijent.
    public partial class OblastiProceneForma : Form
    {
        private readonly ProceniteljBasic _procenitelj;

        public OblastiProceneForma(ProceniteljBasic procenitelj)
        {
            _procenitelj = procenitelj;
            InitializeComponent();
            UcitajOblasti();
        }

        private void UcitajOblasti()
        {
            var trenutne = DTOManager.vratiOblastiZaProcenitelja(_procenitelj.OsobljeId);

            lstOblasti.Items.Clear();
            foreach (var o in trenutne) lstOblasti.Items.Add(new OblastListStavka(o));

            var zauzete = trenutne.Select(o => o.Oblast).ToHashSet();
            cmbNovaOblast.Items.Clear();
            foreach (var kv in UiHelper.NaziviOblastiProcene)
                if (!zauzete.Contains(kv.Key)) cmbNovaOblast.Items.Add(new OblastComboStavka(kv.Key, kv.Value));
            if (cmbNovaOblast.Items.Count > 0) cmbNovaOblast.SelectedIndex = 0;

            bool imaPreostalih = cmbNovaOblast.Items.Count > 0;
            cmbNovaOblast.Visible = imaPreostalih;
            btnDodaj.Visible = imaPreostalih;
            lblSveDodato.Visible = !imaPreostalih;
        }

        private void BtnDodaj_Click(object? sender, EventArgs e)
        {
            if (cmbNovaOblast.SelectedItem is not OblastComboStavka izabrana)
            { MessageBox.Show("Izaberite oblast.", "Validacija"); return; }

            var dto = new OblastProcBasic { ProceniteljId = _procenitelj.OsobljeId, Oblast = izabrana.Kljuc };
            if (!UiHelper.PokusajAkciju(() => DTOManager.dodajOblastProc(dto))) return;
            UcitajOblasti();
        }

        // Nosi i baznu vrednost (kljuc koji ide u bazu) i citljiv naziv (prikazan u comboboxu).
        private readonly record struct OblastComboStavka(string Kljuc, string Naziv)
        {
            public override string ToString() => Naziv;
        }

        // Obavija OblastProcBasic da bi ListBox prikazao citljiv naziv umesto sirovog kljuca iz baze.
        private readonly record struct OblastListStavka(OblastProcBasic Dto)
        {
            public override string ToString() => UiHelper.NazivOblasti(Dto.Oblast);
        }

        private void LstOblasti_DoubleClick(object? sender, EventArgs e)
        {
            if (lstOblasti.SelectedItem is not OblastListStavka stavka) return;
            var o = stavka.Dto;
            if (MessageBox.Show($"Ukloniti oblast \"{UiHelper.NazivOblasti(o.Oblast)}\"?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiOblastProc(o.OblastId))) return;
            UcitajOblasti();
        }
    }
}
