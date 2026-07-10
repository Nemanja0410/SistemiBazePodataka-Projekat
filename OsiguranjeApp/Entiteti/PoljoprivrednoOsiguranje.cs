using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class PoljoprivrednoOsiguranje : Polisa
    {
        public virtual IList<Usev> Usevi { get; set; }
        public virtual IList<Zivotinja> Zivotinje { get; set; }

        public PoljoprivrednoOsiguranje()
        {
            Usevi = new List<Usev>();
            Zivotinje = new List<Zivotinja>();
        }
    }
}
