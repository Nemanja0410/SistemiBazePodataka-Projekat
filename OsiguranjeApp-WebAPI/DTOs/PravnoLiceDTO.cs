using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class PravnoLicePregled : KlijentPregled
    {
        public string? Pib         { get; set; }
        public string? MaticniBroj { get; set; }
        public string? Delatnost   { get; set; }

        public PravnoLicePregled() { }
    }

    public class PravnoLiceBasic : PravnoLicePregled
    {
        public IList<KontaktOsobaBasic> KontaktOsobe { get; set; }

        public PravnoLiceBasic()
        {
            KontaktOsobe = new List<KontaktOsobaBasic>();
        }
    }
}
