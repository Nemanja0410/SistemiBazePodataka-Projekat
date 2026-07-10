namespace OsiguranjApp.DTOs
{
    public class ZivotnoPregled : PolisaPregled
    {
        public decimal SumaOsiguranja { get; set; }
        public string? TipIsplate     { get; set; }

        public ZivotnoPregled() { }
    }
}
