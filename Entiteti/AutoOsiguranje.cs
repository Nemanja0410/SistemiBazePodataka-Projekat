using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class AutoOsiguranje : Polisa
    {
        public virtual Vozilo? Vozilo              { get; set; }
        public virtual string? BonusMalusKlasa     { get; set; }
        public virtual string? TeritorijanoVazenje { get; set; }

        public virtual IList<Klijent> Vozaci { get; set; }

        public AutoOsiguranje()
        {
            Vozaci = new List<Klijent>();
        }
    }
}
