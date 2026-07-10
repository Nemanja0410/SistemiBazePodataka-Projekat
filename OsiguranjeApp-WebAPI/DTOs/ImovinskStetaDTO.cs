namespace OsiguranjApp.DTOs
{
    public class ImovinskStetaPregled : StetaPregled
    {
        public string? ProcenaOstecenja  { get; set; }
        public string? IzvodjacSanacije  { get; set; }

        public ImovinskStetaPregled() { }
    }
}
