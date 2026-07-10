namespace OsiguranjApp.DTOs
{
    public class KontaktOsobaBasic
    {
        public int    KontaktId { get; set; }
        public int    KlijentId { get; set; }
        public string? Ime       { get; set; }
        public string? Prezime   { get; set; }
        public string? Telefon   { get; set; }
        public string? Email     { get; set; }
        public string? Funkcija  { get; set; }

        public KontaktOsobaBasic() { }
        public override string ToString() => $"{Ime} {Prezime}";
    }
}
