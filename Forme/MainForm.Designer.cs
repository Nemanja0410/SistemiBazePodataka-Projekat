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
            this.pnlMenu= new Panel();
            this.lblNaslov= new Label();
            this.btnKlijenti  = new Button();
            this.btnPolise= new Button();
            this.btnStete= new Button();
            this.btnOsoblje= new Button();
            this.btnIzvestaji = new Button();
            this.btnNalozi= new Button();
            this.pnlSesija= new Panel();
            this.lblSesija= new Label();
            this.btnOdjava= new Button();

            this.pnlMenu.SuspendLayout();
            this.pnlSesija.SuspendLayout();
            this.SuspendLayout();

            this.pnlMenu.BackColor = Color.FromArgb(30, 64, 103);
            this.pnlMenu.Dock= DockStyle.Top;
            this.pnlMenu.Height= 60;
            this.pnlMenu.AutoScroll = true;
            this.pnlMenu.Controls.AddRange(new Control[]
            {
                this.lblNaslov, this.btnKlijenti, this.btnPolise,
                this.btnStete, this.btnIzvestaji, this.btnOsoblje, this.btnNalozi
            });

            this.lblNaslov.AutoSize  = true;
            this.lblNaslov.Font= new Font("Segoe UI", 13F);
            this.lblNaslov.ForeColor = Color.White;
            this.lblNaslov.Location  = new Point(14, 16);
            this.lblNaslov.Text= "Osiguravajuća kompanija";

            int x = 310;
            KonfBtnMeni(this.btnKlijenti,  ref x, "👤  Klijenti");
            KonfBtnMeni(this.btnPolise,    ref x, "📋  Polise");
            KonfBtnMeni(this.btnStete,     ref x, "⚠️  Štete");
            KonfBtnMeni(this.btnIzvestaji, ref x, "📊  Izveštaji");
            KonfBtnMeni(this.btnOsoblje,   ref x, "👥  Osoblje");
            KonfBtnMeni(this.btnNalozi,    ref x, "🔑  Nalozi");

            this.btnKlijenti.Click  += btnKlijenti_Click;
            this.btnPolise.Click    += btnPolise_Click;
            this.btnStete.Click     += btnStete_Click;
            this.btnOsoblje.Click   += btnOsoblje_Click;
            this.btnIzvestaji.Click += btnIzvestaji_Click;
            this.btnNalozi.Click    += btnNalozi_Click;

            this.pnlSesija.BackColor = Color.FromArgb(22, 48, 78);
            this.pnlSesija.Dock      = DockStyle.Top;
            this.pnlSesija.Height    = 30;
            this.pnlSesija.Controls.AddRange(new Control[] { this.lblSesija, this.btnOdjava });

            this.btnOdjava.BackColor = Color.FromArgb(192, 57, 43);
            this.btnOdjava.FlatStyle = FlatStyle.Flat;
            this.btnOdjava.FlatAppearance.BorderSize = 0;
            this.btnOdjava.Font      = new Font("Segoe UI", 8.5F);
            this.btnOdjava.ForeColor = Color.White;
            this.btnOdjava.Size      = new Size(90, 24);
            this.btnOdjava.Dock      = DockStyle.Right;
            this.btnOdjava.Text      = "Odjava";
            this.btnOdjava.Cursor    = Cursors.Hand;
            this.btnOdjava.Click    += btnOdjava_Click;

            this.lblSesija.AutoSize  = true;
            this.lblSesija.Font      = new Font("Segoe UI", 8.5F);
            this.lblSesija.ForeColor = Color.FromArgb(200, 210, 225);
            this.lblSesija.Location  = new Point(14, 7);
            this.lblSesija.Text      = "";

            this.BackColor      = Color.FromArgb(240, 242, 245);
            this.ClientSize     = new Size(1280, 800);
            this.Font           = new Font("Segoe UI", 9F);
            this.IsMdiContainer = true;
            this.MinimumSize    = new Size(1024, 650);
            this.StartPosition  = FormStartPosition.CenterScreen;
            this.Text           = "Sistem upravljanja osiguravajućom kompanijom";
            this.Controls.Add(this.pnlSesija);
            this.Controls.Add(this.pnlMenu);

            this.pnlMenu.ResumeLayout(false);
            this.pnlSesija.ResumeLayout(false);
            this.pnlSesija.PerformLayout();
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
        private Button btnNalozi;
        private Panel  pnlSesija;
        private Label  lblSesija;
        private Button btnOdjava;
    }
}
