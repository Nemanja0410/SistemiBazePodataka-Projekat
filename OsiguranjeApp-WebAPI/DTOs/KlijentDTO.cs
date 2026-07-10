using System;
using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class KlijentPregled
    {
        public int      KlijentId          { get; set; }
        public string?  Naziv              { get; set; }
        public string?  TipKlijenta        { get; set; }
        public string?  Adresa             { get; set; }
        public string?  Telefon            { get; set; }
        public string?  Email              { get; set; }
        public string?  Status             { get; set; }
        public DateTime DatumRegistracije  { get; set; }

        public KlijentPregled() { }
        public override string ToString() => Naziv ?? "";
    }

    public class KlijentBasic : KlijentPregled
    {
        public IList<PolisaPregled> Polise { get; set; }
        public IList<StetaPregled>  Stete  { get; set; }

        public KlijentBasic()
        {
            Polise = new List<PolisaPregled>();
            Stete  = new List<StetaPregled>();
        }
    }
}
