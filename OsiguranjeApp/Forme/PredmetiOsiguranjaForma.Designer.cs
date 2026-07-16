using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    partial class PredmetiOsiguranjaForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.Text            = "Predmeti osiguranja";
            this.Size            = new Size(820, 580);
            this.MinimumSize     = new Size(600, 420);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = UiHelper.PozadinaForm;

            var naslov = UiHelper.NapraviNaslov("🌾  Predmeti osiguranja");

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(NapraviUsevZivotinjaTab(
                "Usevi",
                () => DTOManager.vratiSveUseve(),
                dto => DTOManager.dodajUsev(dto),
                dto => DTOManager.azurirajUsev(dto),
                id  => DTOManager.obrisiUsev(id)));
            tabs.TabPages.Add(NapraviUsevZivotinjaTab(
                "Životinje",
                () => DTOManager.vratiSveZivotinje().ConvertAll(z => new UsevBasic
                    { UsevId = z.ZivotinjaId, Vrsta = z.Vrsta, Lokacija = z.Lokacija, ProcenjenaVrednost = z.ProcenjenaVrednost }),
                dto => DTOManager.dodajZivotinju(new OsiguranjApp.DTOs.ZivotinjaBasic { Vrsta = dto.Vrsta, Lokacija = dto.Lokacija, ProcenjenaVrednost = dto.ProcenjenaVrednost }),
                dto => DTOManager.azurirajZivotinju(new OsiguranjApp.DTOs.ZivotinjaBasic { ZivotinjaId = dto.UsevId, Vrsta = dto.Vrsta, Lokacija = dto.Lokacija, ProcenjenaVrednost = dto.ProcenjenaVrednost }),
                id  => DTOManager.obrisiZivotinju(id)));
            tabs.TabPages.Add(NapraviPokretnaImovinaTab());
            tabs.TabPages.Add(NapraviVozilaTab());
            tabs.TabPages.Add(NapraviNekretnineTab());

            this.Controls.Add(tabs);
            this.Controls.Add(naslov);
        }
    }
}
