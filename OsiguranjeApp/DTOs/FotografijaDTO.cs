using System;

namespace OsiguranjApp.DTOs
{
    public class FotografijaBasic
    {
        public int      FotografijaId   { get; set; }
        public int      StetaId         { get; set; }
        public string?  Putanja         { get; set; }
        public string?  Opis            { get; set; }
        public DateTime DatumDodavanja  { get; set; }

        public FotografijaBasic() { }
    }
}
