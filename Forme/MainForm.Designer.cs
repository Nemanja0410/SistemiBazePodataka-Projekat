using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlMenu      = new Panel();
            this.lblNaslov    = new Label();
            this.btnKlijenti  = new Button();
            this.btnPolise    = new Button();
            this.btnStete     = new Button();
            this.btnOsoblje   = new Button();
            this.btnIzvestaji = new Button();

            this.pnlMenu.SuspendLayout();
            this.SuspendLayout();

            this.pnlMenu.BackColor = Color.FromArgb(30, 64, 103);
            this.pnlMenu.Dock      = DockStyle.Top;
            this.pnlMenu.Height    = 60;
            this.pnlMenu.Controls.AddRange(new Control[]
            {
                this.lblNaslov, this.btnKlijenti, this.btnPolise,
                this.btnStete, this.btnOsoblje, this.btnIzvestaji
            });

            this.lblNaslov.AutoSize  = true;
            this.lblNaslov.Font      = new Font("Segoe UI", 13F);
            this.lblNaslov.ForeColor = Color.White;
            this.lblNaslov.Location  = new Point(14, 16);
            this.lblNaslov.Text      = "Osiguravajuća kompanija";

            int x = 310;
            KonfBtnMeni(this.btnKlijenti,  ref x, "👤  Klijenti");
            KonfBtnMeni(this.btnPolise,    ref x, "📋  Polise");
            KonfBtnMeni(this.btnStete,     ref x, "⚠️  Štete");
            KonfBtnMeni(this.btnOsoblje,   ref x, "👥  Osoblje");
            KonfBtnMeni(this.btnIzvestaji, ref x, "📊  Izveštaji");

            this.btnKlijenti.Click  += btnKlijenti_Click;
            this.btnPolise.Click    += btnPolise_Click;
            this.btnStete.Click     += btnStete_Click;
            this.btnOsoblje.Click   += btnOsoblje_Click;
            this.btnIzvestaji.Click += btnIzvestaji_Click;

            this.BackColor      = Color.FromArgb(240, 242, 245);
            this.ClientSize     = new Size(1280, 800);
            this.Font           = new Font("Segoe UI", 9F);
            this.IsMdiContainer = true;
            this.MinimumSize    = new Size(1024, 650);
            this.StartPosition  = FormStartPosition.CenterScreen;
            this.Text           = "Sistem upravljanja osiguravajućom kompanijom";
            this.Controls.Add(this.pnlMenu);

            this.pnlMenu.ResumeLayout(false);
            this.pnlMenu.PerformLayout();
            this.ResumeLayout(false);
        }

        private void KonfBtnMeni(Button btn, ref int x, string tekst)
        {
            btn.BackColor = Color.FromArgb(52, 120, 186);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font      = new Font("Segoe UI", 9.5F);
            btn.ForeColor = Color.White;
            btn.Location  = new Point(x, 12);
            btn.Size      = new Size(130, 36);
            btn.Text      = tekst;
            btn.Cursor    = Cursors.Hand;
            x += 140;
        }

        private Panel  pnlMenu;
        private Label  lblNaslov;
        private Button btnKlijenti;
        private Button btnPolise;
        private Button btnStete;
        private Button btnOsoblje;
        private Button btnIzvestaji;
    }
}
