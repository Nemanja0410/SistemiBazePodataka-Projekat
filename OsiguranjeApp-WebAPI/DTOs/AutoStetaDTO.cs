namespace OsiguranjApp.DTOs
{
    public class AutoStetaPregled : StetaPregled
    {
        public string? ZapisnikPolicije { get; set; }
        public string? Servis           { get; set; }
        public int?    VoziloId         { get; set; }
        public string? VoziloOpis       { get; set; }

        public AutoStetaPregled() { }
    }
}
