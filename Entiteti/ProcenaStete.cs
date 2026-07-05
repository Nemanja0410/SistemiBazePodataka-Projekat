using System;

namespace OsiguranjApp.Entiteti
{
    public class ProcenaStete
    {
        public virtual int         ProcenaId       { get; set; }
        public virtual Steta?       Steta           { get; set; }
        public virtual DateTime     DatumProc       { get; set; }
        public virtual Procenitelj? Procenitelj     { get; set; }
        public virtual string?      MetodProc       { get; set; }
        public virtual string?      Nalaz           { get; set; }
        public virtual decimal      ProcenjeniIznos { get; set; }
        public virtual string?      Preporuka       { get; set; }
    }
}
