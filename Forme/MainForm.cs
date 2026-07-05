using System.Windows.Forms;
using OsiguranjApp.Forme;

namespace OsiguranjApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnKlijenti_Click(object sender, System.EventArgs e)
        {
            OtvoriFormu(new KlijentiForma());
        }

        private void btnPolise_Click(object sender, System.EventArgs e)
        {
            OtvoriFormu(new PoliseForma());
        }

        private void btnStete_Click(object sender, System.EventArgs e)
        {
            OtvoriFormu(new SteteForma());
        }

        private void btnOsoblje_Click(object sender, System.EventArgs e)
        {
            OtvoriFormu(new OsobljeForma());
        }

        private void btnIzvestaji_Click(object sender, System.EventArgs e)
        {
            OtvoriFormu(new IzvestajiForma());
        }

        private void OtvoriFormu(Form forma)
        {
            foreach (Form f in this.MdiChildren)
            {
                if (f.GetType() == forma.GetType())
                {
                    f.Activate();
                    forma.Dispose();
                    return;
                }
            }
            forma.MdiParent   = this;
            forma.WindowState = FormWindowState.Maximized;
            forma.Show();
        }
    }
}
