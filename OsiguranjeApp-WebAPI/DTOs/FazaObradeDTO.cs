using System;

namespace OsiguranjApp.DTOs
{
    public class FazaObradeBasic
    {
        public int      FazaId           { get; set; }
        public int      StetaId          { get; set; }
        public int      RedniBrojFaze    { get; set; }
        public string?  NazivFaze        { get; set; }
        public DateTime DatumPocetka     { get; set; }
        public DateTime? DatumZavrsetka  { get; set; }
        public int?     OdgovornoLiceId  { get; set; }
        public string?  OdgovornoLiceIme { get; set; }
        public string?  Odluka           { get; set; }
        public string?  Dokumentacija    { get; set; }
        public string?  Napomena         { get; set; }

        public FazaObradeBasic() { }
    }
}
