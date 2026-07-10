using System;

namespace OsiguranjApp.DTOs
{
    public class FizickoLicePregled : KlijentPregled
    {
        public string?   Jmbg           { get; set; }
        public DateTime? DatumRodjenja  { get; set; }
        public string?   Zanimanje      { get; set; }

        public FizickoLicePregled() { }
    }

    public class FizickoLiceBasic : FizickoLicePregled
    {
        public FizickoLiceBasic() { }
    }
}
