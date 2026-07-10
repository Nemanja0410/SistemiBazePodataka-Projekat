using System.Linq;
using System.Threading;
using OsiguranjApp.DTOs;

namespace OsiguranjApp
{
    // U Web API-ju server istovremeno opslužuje zahteve više različitih korisnika,
    // pa obična "static" promenljiva ovde ne sme da se koristi (jedan korisnik bi mogao
    // da vidi/menja podatke kao drugi). AsyncLocal drži vrednost izolovanu po HTTP zahtevu
    // (svaki zahtev ima sopstveni async kontekst), a u WinForms desktop aplikaciji se i dalje
    // ponaša identično kao obično static polje.
    public static class SesijaKorisnik
    {
        private static readonly AsyncLocal<NalogPregled?> _trenutniNalog = new AsyncLocal<NalogPregled?>();
        private static readonly AsyncLocal<bool> _zahtevZaOdjavu = new AsyncLocal<bool>();

        public static NalogPregled? TrenutniNalog
        {
            get => _trenutniNalog.Value;
            set => _trenutniNalog.Value = value;
        }

        public static bool ZahtevZaOdjavu
        {
            get => _zahtevZaOdjavu.Value;
            set => _zahtevZaOdjavu.Value = value;
        }

        public static bool JeUlogovan => TrenutniNalog != null;

        public static bool ImaUlogu(params string[] uloge) =>
            TrenutniNalog != null && uloge.Contains(TrenutniNalog.Uloga);

        public static void Odjava() => TrenutniNalog = null;
    }
}
