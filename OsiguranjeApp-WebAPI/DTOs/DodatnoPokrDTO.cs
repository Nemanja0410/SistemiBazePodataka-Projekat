namespace OsiguranjApp.DTOs
{
    public class DodatnoPokrBasic
    {
        public int     PokriceId      { get; set; }
        public int     PolisaId       { get; set; }
        public string? Naziv          { get; set; }
        public string? Opis           { get; set; }
        public decimal? LimitPokrića  { get; set; }
        public decimal Fransiza       { get; set; }
        public decimal DodatnaPremija { get; set; }

        public DodatnoPokrBasic() { }
        public override string ToString() => Naziv ?? "";
    }
}
