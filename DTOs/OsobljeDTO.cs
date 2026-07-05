using System;

namespace OsiguranjApp.DTOs
{
    public class OsobljePregled
    {
        public int      OsobljeId         { get; set; }
        public string?  Ime               { get; set; }
        public string?  Prezime           { get; set; }
        public string?  Jmbg              { get; set; }
        public string?  Adresa            { get; set; }
        public string?  Telefon           { get; set; }
        public string?  Email             { get; set; }
        public DateTime DatumAngazovanja  { get; set; }
        public string?  Status            { get; set; }
        public string?  TipOsoblja        { get; set; }

        public OsobljePregled() { }
        public override string ToString() => $"{Ime} {Prezime}";
    }

    public class OsobljeBasic : OsobljePregled
    {
        public string? TipPravnika { get; set; }
        public string? BarBroj     { get; set; }

        public OsobljeBasic() { }
    }
}
