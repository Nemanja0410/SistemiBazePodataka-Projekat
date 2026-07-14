using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class AgentPregled : OsobljePregled
    {
        public string? TipAgenta          { get; set; }
        public string? Licenca            { get; set; }
        public string? RegionRada         { get; set; }
        public decimal ProvizijaProcenat  { get; set; }
        public int     BrojPolisa         { get; set; }
        public decimal UkupnaPremija      { get; set; }

        public AgentPregled() { }
    }

    public class AgentBasic : AgentPregled
    {
        public IList<PolisaPregled> Polise { get; set; }

        public AgentBasic()
        {
            Polise = new List<PolisaPregled>();
        }

        public override string ToString() => $"{Ime} {Prezime}";
    }
}
