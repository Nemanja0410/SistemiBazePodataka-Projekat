using System;
using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class Klijent
    {
        public virtual int      KlijentId          { get; set; }
        public virtual string?  Naziv              { get; set; }
        public virtual string?  Adresa             { get; set; }
        public virtual string?  Telefon            { get; set; }
        public virtual string?  Email              { get; set; }
        public virtual string?  Status             { get; set; }
        public virtual DateTime DatumRegistracije  { get; set; }
        public virtual string?  TipKlijenta        { get; set; }

        public virtual IList<Polisa>       Polise  { get; set; }
        public virtual IList<Steta>        Stete   { get; set; }

        public Klijent()
        {
            Polise = new List<Polisa>();
            Stete  = new List<Steta>();
        }

        public override string ToString() => Naziv ?? "";
    }
}
