using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class JavnaInstitucijaPregled : KlijentPregled
    {
        public string? Pib             { get; set; }
        public string? MaticniBroj     { get; set; }
        public string? Delatnost       { get; set; }
        public string? NivoInstitucije { get; set; }

        public JavnaInstitucijaPregled() { }
    }

    public class JavnaInstitucijaBasic : JavnaInstitucijaPregled
    {
        public IList<KontaktOsobaBasic> KontaktOsobe { get; set; }

        public JavnaInstitucijaBasic()
        {
            KontaktOsobe = new List<KontaktOsobaBasic>();
        }
    }
}
