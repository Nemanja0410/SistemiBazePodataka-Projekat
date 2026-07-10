using System;
using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class PolisaPregled
    {
        public int      PolisaId        { get; set; }
        public string?  BrojPolise      { get; set; }
        public string?  TipOsiguranja   { get; set; }
        public DateTime DatumPocetka    { get; set; }
        public DateTime DatumIsteka     { get; set; }
        public string?  Status          { get; set; }
        public decimal  OsnovnaPremija  { get; set; }
        public string?  Valuta          { get; set; }
        public string?  NacinPlacanja   { get; set; }
        public int      UgovaracId      { get; set; }
        public string?  UgovaracNaziv   { get; set; }
        public int?     AgentId         { get; set; }
        public string?  AgentIme        { get; set; }

        public PolisaPregled() { }
        public override string ToString() => BrojPolise ?? "";
    }

    public class PolisaBasic : PolisaPregled
    {
        public IList<DodatnoPokrBasic> DodatnaPokrića { get; set; }

        public PolisaBasic()
        {
            DodatnaPokrića = new List<DodatnoPokrBasic>();
        }
    }
}
