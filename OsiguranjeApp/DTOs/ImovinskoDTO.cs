using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class ImovinskoPregled : PolisaPregled
    {
        public string?   VrsteRizika          { get; set; }
        public List<int> NekretnineIds        { get; set; }
        public List<int> PokretnaImovinaIds   { get; set; }

        public ImovinskoPregled()
        {
            NekretnineIds = new List<int>();
            PokretnaImovinaIds = new List<int>();
        }
    }
}
