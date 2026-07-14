namespace OsiguranjApp.Entiteti
{
    public class DodatnoPokrice
    {
        public virtual int PokriceId { get; set; }
        public virtual Polisa? Polisa { get; set; }
        public virtual string? Naziv { get; set; }
        public virtual string? Opis { get; set; }
        public virtual decimal? LimitPokrića { get; set; }
        public virtual decimal Fransiza { get; set; }
        public virtual decimal DodatnaPremija { get; set; }
        public override string ToString() => Naziv ?? "";
    } 
}
