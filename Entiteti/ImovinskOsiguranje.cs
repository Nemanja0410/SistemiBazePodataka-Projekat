using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class ImovinskOsiguranje : Polisa
    {
        public virtual string? VrsteRizika { get; set; }

        public virtual IList<Nekretnina> Nekretnine { get; set; }

        public ImovinskOsiguranje()
        {
            Nekretnine = new List<Nekretnina>();
        }
    }
}
