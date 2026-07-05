using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class ProceniteljBasic : OsobljeBasic
    {
        public IList<string> Oblasti { get; set; }

        public ProceniteljBasic()
        {
            Oblasti = new List<string>();
        }
    }
}
