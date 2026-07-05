namespace OsiguranjApp.Entiteti
{
    public class AutoSteta : Steta
    {
        public virtual string? ZapisnikPolicije { get; set; }
        public virtual string? Servis           { get; set; }
        public virtual Vozilo? Vozilo           { get; set; }
    }
}
