using System;
using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class ProcenaStetaBasic
    {
        public int      ProcenaId              { get; set; }
        public int      StetaId                { get; set; }
        public DateTime DatumProc               { get; set; }
        public int      ProceniteljId           { get; set; }
        public string?  ProceniteljIme          { get; set; }
        public string?  ProceniteljBrojLicence  { get; set; }
        public IList<string> ProceniteljOblasti { get; set; }
        public string?  MetodProc               { get; set; }
        public string?  Nalaz                   { get; set; }
        public decimal  ProcenjeniIznos         { get; set; }
        public string?  Preporuka               { get; set; }

        public ProcenaStetaBasic()
        {
            ProceniteljOblasti = new List<string>();
        }
    }
}
