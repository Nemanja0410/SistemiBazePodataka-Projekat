using System.Collections.Generic;

namespace OsiguranjApp.DTOs
{
    public class ProceniteljBasic : OsobljeBasic
    {
        public IList<string> Oblasti { get; set; }
        public IList<ProcenaStetaBasic> Procene { get; set; }

        public ProceniteljBasic()
        {
            Oblasti = new List<string>();
            Procene = new List<ProcenaStetaBasic>();
        }
    }
}
