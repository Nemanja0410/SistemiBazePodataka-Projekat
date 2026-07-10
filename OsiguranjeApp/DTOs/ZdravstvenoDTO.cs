namespace OsiguranjApp.DTOs
{
    public class ZdravstvenoPregled : PolisaPregled
    {
        public string? MrezaUstanova     { get; set; }
        public decimal LimitSpecijalista { get; set; }
        public decimal LimitStomatologa  { get; set; }
        public decimal LimitBolnickih    { get; set; }
        public decimal LimitBolnickiDan  { get; set; }

        public ZdravstvenoPregled() { }
    }
}
