using System;

namespace OsiguranjApp.Entiteti
{
    public class IstorijaPolise
    {
        public virtual int      IstorijaId   { get; set; }
        public virtual Polisa?  Polisa       { get; set; }
        public virtual string?  TipPromene   { get; set; }
        public virtual DateTime DatumPromene { get; set; }
        public virtual string?  Opis         { get; set; }
        public virtual Osoblje? Korisnik     { get; set; }
    }
}
