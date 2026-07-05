namespace OsiguranjApp.Entiteti
{
    public class OstecenLice
    {
        public virtual int     OstecenLiceId  { get; set; }
        public virtual Steta?  Steta          { get; set; }
        public virtual Klijent? Klijent       { get; set; }
        public virtual string?  ImePrezime    { get; set; }
        public virtual string?  OpisPovrede   { get; set; }
        public virtual decimal? IznosNaknade  { get; set; }
    }
}
