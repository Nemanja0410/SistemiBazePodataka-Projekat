namespace OsiguranjApp.DTOs
{
    public class PrijavaZahtev
    {
        public string? KorisnickoIme { get; set; }
        public string? Lozinka { get; set; }
    }

    public class PromenaLozinkeZahtev
    {
        public string? StaraLozinka { get; set; }
        public string? NovaLozinka { get; set; }
    }

    public class ResetLozinkeZahtev
    {
        public string? PrivremenaLozinka { get; set; }
    }

    public class OdobrenjeZahtev
    {
        public string? DodeljenaUloga { get; set; }
    }
}
