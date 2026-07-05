using System;

namespace OsiguranjApp.DTOs
{
    public class ProcenaStetaBasic
    {
        public int      ProcenaId       { get; set; }
        public int      StetaId         { get; set; }
        public DateTime DatumProc       { get; set; }
        public int      ProceniteljId   { get; set; }
        public string?  ProceniteljIme  { get; set; }
        public string?  MetodProc       { get; set; }
        public string?  Nalaz           { get; set; }
        public decimal  ProcenjeniIznos { get; set; }
        public string?  Preporuka       { get; set; }

        public ProcenaStetaBasic() { }
    }
}
