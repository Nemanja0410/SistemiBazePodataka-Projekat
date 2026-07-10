using System;

namespace OsiguranjApp.Entiteti
{
    public class Nalog
    {
        public virtual int NalogId  { get; set; }
        public virtual string? KorisnickoIme { get; set; }
        public virtual string? LozinkaHash { get; set; }
        public virtual string?  LozinkaSalt { get; set; }
        public virtual int  LozinkaIteracije { get; set; }
        public virtual Osoblje? Osoblje { get; set; }
        public virtual string? Uloga { get; set; }
        public virtual string? StatusNaloga { get; set; }
        public virtual int NeuspesnihPrijava { get; set; }
        public virtual bool MoraPromenitiLozinku  { get; set; }
        public virtual DateTime DatumRegistracije { get; set; }
        public virtual DateTime? DatumOdobrenja { get; set; }
        public virtual DateTime? ZadnjaPrijava { get; set; }
        public virtual Nalog? OdobrioNalog { get; set; }
    }
}
