namespace OsiguranjApp.Entiteti
{
    public class KorisnikIsplate
    {
        public virtual int  KorisnikId { get; set; }
        public virtual ZivotnoOsiguranje? Polisa { get; set; }
        public virtual Klijent? Klijent { get; set; }
        public virtual string? ImePrezime { get; set; }
        public virtual decimal ProcenatUdela { get; set; }
    }
}
