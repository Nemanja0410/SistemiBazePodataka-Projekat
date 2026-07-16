using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class IzvestajiForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TabControl tabControl = null!;

        private void InitializeComponent()
        {
            this.Text      = "Izveštaji i statistike";
            this.Size      = new Size(1050, 680);
            this.BackColor = UiHelper.PozadinaForm;
            this.Font      = new Font("Segoe UI", 9f);

            var naslov = UiHelper.NapraviNaslov("📊  Izveštaji i statistike");

            tabControl = new TabControl
            {
                Dock    = DockStyle.Fill,
                Font    = new Font("Segoe UI", 9.5f),
                Padding = new System.Drawing.Point(12, 6)
            };

            tabControl.TabPages.Add(KreirajTabPolise());
            tabControl.TabPages.Add(KreirajTabStete());
            tabControl.TabPages.Add(KreirajTabAgenti());
            tabControl.TabPages.Add(KreirajTabIsticu());
            tabControl.TabPages.Add(KreirajTabSirovi());

            this.Controls.Add(tabControl);
            this.Controls.Add(naslov);
        }
    }
}
