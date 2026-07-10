using System;
using System.Windows.Forms;
using OsiguranjApp.Forme;

namespace OsiguranjApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            bool nastavi = true;
            while (nastavi)
            {
                using var login = new LoginForma();
                if (login.ShowDialog() != DialogResult.OK || !SesijaKorisnik.JeUlogovan)
                    return;

                SesijaKorisnik.ZahtevZaOdjavu = false;
                Application.Run(new MainForm());
                nastavi = SesijaKorisnik.ZahtevZaOdjavu;
            }
        }
    }
}
