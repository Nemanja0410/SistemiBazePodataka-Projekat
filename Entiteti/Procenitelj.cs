using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class Procenitelj : Osoblje
    {
        public virtual IList<OblastProc>   OblasiProc   { get; set; }
        public virtual IList<ProcenaStete> ProceneSteta { get; set; }

        public Procenitelj()
        {
            OblasiProc   = new List<OblastProc>();
            ProceneSteta = new List<ProcenaStete>();
        }
    }
}
