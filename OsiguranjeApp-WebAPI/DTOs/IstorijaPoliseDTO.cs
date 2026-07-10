using System;

namespace OsiguranjApp.DTOs
{
    public class IstorijaPoliseBasic
    {
        public int      IstorijaId   { get; set; }
        public int      PolisaId     { get; set; }
        public string?  TipPromene   { get; set; }
        public DateTime DatumPromene { get; set; }
        public string?  Opis         { get; set; }
        public int?     KorisnikId   { get; set; }
        public string?  KorisnikIme  { get; set; }

        public IstorijaPoliseBasic() { }
    }
}
