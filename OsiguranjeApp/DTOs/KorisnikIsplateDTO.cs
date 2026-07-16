namespace OsiguranjApp.DTOs
{
    public class KorisnikIsplateBasic
    {
        public int     KorisnikId    { get; set; }
        public int     PolisaId      { get; set; }
        public int?    KlijentId     { get; set; }
        public string? KlijentNaziv  { get; set; }
        public string? ImePrezime    { get; set; }
        public decimal ProcenatUdela { get; set; }

        public KorisnikIsplateBasic() { }
        public override string ToString() => KlijentNaziv ?? ImePrezime ?? "";
    }
}
