using System;

namespace OsiguranjApp.Entiteti
{
    public class Fotografija
    {
        public virtual int FotografijaId { get; set; }
        public virtual Steta? Steta { get; set; }
        public virtual string? Putanja { get; set; }
        public virtual string? Opis { get; set; }
        public virtual DateTime DatumDodavanja { get; set; }

        public Fotografija()
        {
            DatumDodavanja = DateTime.Now;
        }
    }
}
