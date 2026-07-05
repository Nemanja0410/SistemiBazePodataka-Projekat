using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class ZivotnoOsiguranje : Polisa
    {
        public virtual decimal SumaOsiguranja { get; set; }
        public virtual string? TipIsplate     { get; set; }

        public virtual IList<KorisnikIsplate> KorisniciIsplate { get; set; }

        public ZivotnoOsiguranje()
        {
            KorisniciIsplate = new List<KorisnikIsplate>();
        }
    }
}
