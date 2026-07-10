namespace OsiguranjApp.DTOs
{
    public class SpecijalizovanoPregled : PolisaPregled
    {
        public string? NazivSpecijalizacije { get; set; }
        public string? OpisUslova           { get; set; }

        public SpecijalizovanoPregled() { }
    }
}
