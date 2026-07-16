namespace OsiguranjApp.DTOs
{
    public class ImovinskStetaBasic : StetaBasic
    {
        public string? ProcenaOstecenja { get; set; }
        public string? IzvodjacSanacije { get; set; }

        public ImovinskStetaBasic() { }
    }
}
