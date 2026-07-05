using System;

namespace OsiguranjApp.Entiteti
{
    public class FazaObrade
    {
        public virtual int      FazaId          { get; set; }
        public virtual Steta?   Steta           { get; set; }
        public virtual int      RedniBrojFaze   { get; set; }
        public virtual string?  NazivFaze       { get; set; }
        public virtual DateTime DatumPocetka    { get; set; }
        public virtual DateTime? DatumZavrsetka { get; set; }
        public virtual Osoblje? OdgovornoLice   { get; set; }
        public virtual string?  Odluka          { get; set; }
        public virtual string?  Dokumentacija   { get; set; }
        public virtual string?  Napomena        { get; set; }
    }
}
