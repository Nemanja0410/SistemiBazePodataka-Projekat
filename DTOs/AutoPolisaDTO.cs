namespace OsiguranjApp.DTOs
{
    public class AutoPolisaPregled : PolisaPregled
    {
        public int    VoziloId            { get; set; }
        public string? VoziloOpis          { get; set; }
        public string? BonusMalusKlasa     { get; set; }
        public string? TeritorijanoVazenje { get; set; }

        public AutoPolisaPregled() { }
    }
}
