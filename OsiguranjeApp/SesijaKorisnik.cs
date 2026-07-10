using System.Linq;
using OsiguranjApp.DTOs;

namespace OsiguranjApp
{
    public static class SesijaKorisnik
    {
        public static NalogPregled? TrenutniNalog { get; set; }
        public static bool ZahtevZaOdjavu { get; set; }

        public static bool JeUlogovan => TrenutniNalog != null;

        public static bool ImaUlogu(params string[] uloge) =>
            TrenutniNalog != null && uloge.Contains(TrenutniNalog.Uloga);

        public static void Odjava() => TrenutniNalog = null;
    }
}
