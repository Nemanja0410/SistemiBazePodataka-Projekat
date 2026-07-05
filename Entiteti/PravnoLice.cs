using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class PravnoLice : Klijent
    {
        public virtual string? Pib         { get; set; }
        public virtual string? MaticniBroj { get; set; }
        public virtual string? Delatnost   { get; set; }

        public virtual IList<KontaktOsoba> KontaktOsobe { get; set; }

        public PravnoLice()
        {
            KontaktOsobe = new List<KontaktOsoba>();
        }
    }
}
