using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class ImovinskoPregled : PolisaPregled
    {
        public string? VrsteRizika { get; set; }
        public List<int> NekretnineIds { get; set; }
        public List<int> PokretneImovineIds { get; set; }

        public ImovinskoPregled()
        {
            NekretnineIds = new List<int>();
            PokretneImovineIds = new List<int>();
        }
    }
}
