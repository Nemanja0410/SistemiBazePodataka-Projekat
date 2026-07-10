using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class PoljoprivrednoPregled : PolisaPregled
    {
        public List<int> UseviIds     { get; set; }
        public List<int> ZivotinjeIds { get; set; }

        public PoljoprivrednoPregled()
        {
            UseviIds = new List<int>();
            ZivotinjeIds = new List<int>();
        }
    }
}
