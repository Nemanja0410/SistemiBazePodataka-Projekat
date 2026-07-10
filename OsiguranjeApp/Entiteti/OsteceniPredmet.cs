namespace OsiguranjApp.Entiteti
{
    public class OsteceniPredmet
    {
        public virtual int OsteceniPredmetId { get; set; }
        public virtual Steta?  Steta { get; set; }
        public virtual string? TipPredmeta { get; set; }
        public virtual string? OpisOstecenja { get; set; }
        public virtual decimal? ProcenjeniIznos { get; set; }
    }
}
