using System;
using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class PutnoPregled : PolisaPregled
    {
        public string?   Destinacije      { get; set; }
        public DateTime  DatumPolaska     { get; set; }
        public DateTime  DatumPovratka    { get; set; }
        public List<int> OsiguranaLicaIds { get; set; }

        public PutnoPregled()
        {
            OsiguranaLicaIds = new List<int>();
        }
    }
}
