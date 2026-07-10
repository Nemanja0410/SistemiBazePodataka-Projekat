using System.Windows.Forms;
using OsiguranjApp.Forme;

namespace OsiguranjApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            PrimeniUlogu();
        }

        private void PrimeniUlogu()
        {
            var nalog = SesijaKorisnik.TrenutniNalog;
            if (nalog == null) return;

            lblSesija.Text = nalog.ImeOsoblja != null
                ? $"Ulogovan kao: {nalog.ImeOsoblja} {nalog.PrezimeOsoblja} ({nalog.Uloga})"
                : $"Ulogovan kao: {nalog.KorisnickoIme} ({nalog.Uloga})";

            bool jeAdmin = SesijaKorisnik.ImaUlogu("ADMIN");
            btnOsoblje.Visible = jeAdmin;
            btnNalozi.Visible  = jeAdmin;
        }

        private void btnNalozi_Click(object sender, System.EventArgs e)
        {
            OtvoriFormu(new UpravljanjeNalozimaForma());
        }

        private void btnOdjava_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Odjaviti se?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            SesijaKorisnik.Odjava();
            SesijaKorisnik.ZahtevZaOdjavu = true;
            this.Close();
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
