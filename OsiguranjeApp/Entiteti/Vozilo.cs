using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class Vozilo
    {
        public virtual int VoziloId { get; set; }
        public virtual string?  Registracija { get; set; }
        public virtual string?  Marka { get; set; }
        public virtual string?  Model { get; set; }
        public virtual int  GodinaProizvodnje { get; set; }
        public virtual Klijent? Vlasnik { get; set; }
        public virtual IList<AutoOsiguranje> Osiguranja { get; set; }
        public Vozilo()
        {
            Osiguranja = new List<AutoOsiguranje>();
        }

        public override string ToString() => $"{Registracija} – {Marka} {Model}";
    }
}
