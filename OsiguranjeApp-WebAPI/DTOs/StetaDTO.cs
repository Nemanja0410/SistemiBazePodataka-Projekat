using System;
using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class StetaPregled
    {
        public int      StetaId          { get; set; }
        public string?  BrojStete        { get; set; }
        public DateTime DatumPrijave     { get; set; }
        public DateTime DatumNastanka    { get; set; }
        public int      PolisaId         { get; set; }
        public string?  BrojPolise       { get; set; }
        public int      PodnosilacId     { get; set; }
        public string?  PodnosilacNaziv  { get; set; }
        public string?  VrstaStete       { get; set; }
        public string?  OpisDogodjaja    { get; set; }
        public string?  Lokacija         { get; set; }
        public string?  Status           { get; set; }
        public decimal? ProcenjeniIznos  { get; set; }
        public string?  Valuta           { get; set; }
        public IList<FazaObradeBasic>   FazeObrade   { get; set; }
        public IList<ProcenaStetaBasic> ProceneSteta { get; set; }

        public StetaPregled()
        {
            FazeObrade   = new List<FazaObradeBasic>();
            ProceneSteta = new List<ProcenaStetaBasic>();
        }
        public override string ToString() => BrojStete ?? "";
    }

    public class StetaBasic : StetaPregled
    {
        public StetaBasic() { }
    }
}
