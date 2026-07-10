using System.Linq;

namespace OsiguranjApp
{
    public partial class DTOManager
    {
        public static void ProveriOvlascenje(params string[] dozvoljeneUloge)
        {
            if (!SesijaKorisnik.JeUlogovan)
                throw new NeovlascenPristupException("Niste prijavljeni.");

            if (!dozvoljeneUloge.Contains(SesijaKorisnik.TrenutniNalog!.Uloga))
                throw new NeovlascenPristupException();
        }
    }
}
