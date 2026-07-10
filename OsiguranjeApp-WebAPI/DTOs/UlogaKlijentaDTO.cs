namespace OsiguranjApp.DTOs
{
    public class UlogaKlijentaBasic
    {
        public int     UlogaId      { get; set; }
        public int     KlijentId    { get; set; }
        public string? KlijentNaziv { get; set; }
        public int     PolisaId     { get; set; }
        public string? BrojPolise   { get; set; }
        public string? TipUloge     { get; set; }

        public UlogaKlijentaBasic() { }
    }
}
