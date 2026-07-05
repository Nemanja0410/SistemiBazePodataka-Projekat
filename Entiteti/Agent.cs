using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class Agent : Osoblje
    {
        public virtual string? TipAgenta          { get; set; }
        public virtual string? Licenca            { get; set; }
        public virtual string? RegionRada         { get; set; }
        public virtual decimal ProvizijaProcenat  { get; set; }

        public virtual IList<Polisa> Polise { get; set; }
        public virtual IList<Steta>  Stete  { get; set; }

        public Agent()
        {
            Polise = new List<Polisa>();
            Stete  = new List<Steta>();
        }
    }
}
