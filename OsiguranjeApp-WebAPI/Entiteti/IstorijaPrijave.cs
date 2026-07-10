using System;

namespace OsiguranjApp.Entiteti
{
    public class IstorijaPrijave
    {
        public virtual int IstorijaPrijaveId { get; set; }
        public virtual Nalog? Nalog { get; set; }
        public virtual string? KorisnickoImePokusaj { get; set; }
        public virtual DateTime VremePokusaja { get; set; }
        public virtual bool Uspesno { get; set; }
        public virtual string? Razlog { get; set; }
    }
}
