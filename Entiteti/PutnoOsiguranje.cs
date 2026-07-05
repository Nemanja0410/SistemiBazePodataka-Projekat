using System;
using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class PutnoOsiguranje : Polisa
    {
        public virtual string?  Destinacije    { get; set; }
        public virtual DateTime DatumPolaska   { get; set; }
        public virtual DateTime DatumPovratka  { get; set; }

        public virtual IList<Klijent> OsiguranaLica { get; set; }

        public PutnoOsiguranje()
        {
            OsiguranaLica = new List<Klijent>();
        }
    }
}
