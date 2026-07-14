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
        // Ovde (a ne samo u PolisaBasic) da bi je nasledili i AutoPolisaPregled/ZivotnoPregled/itd,
        // pa vratiPolisuDetaljno moze da je popuni bez obzira na konkretan podtip polise.
        public IList<DodatnoPokrBasic> DodatnaPokrića { get; set; }

        public PolisaPregled()
        {
            DodatnaPokrića = new List<DodatnoPokrBasic>();
        }
        public override string ToString() => BrojPolise ?? "";
    }

    public class PolisaBasic : PolisaPregled
    {
        public PolisaBasic() { }
    }
}
