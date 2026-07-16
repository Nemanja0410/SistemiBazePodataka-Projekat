namespace OsiguranjApp.Entiteti
{
    public class KorisnikIsplate
    {
        public virtual int  KorisnikId { get; set; }
        public virtual ZivotnoOsiguranje? Polisa { get; set; }
        public virtual Klijent? Klijent { get; set; }
        public virtual decimal ProcenatUdela { get; set; }
        // Korisnik isplate moze biti registrovan klijent ili slobodan unos imena.
        public virtual string? ImePrezime { get; set; }
    }
}
