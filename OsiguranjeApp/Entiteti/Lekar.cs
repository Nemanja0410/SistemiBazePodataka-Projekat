using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class Lekar : Osoblje
    {
        public virtual string? Specijalizacija { get; set; }
        public virtual string? LicencaBroj { get; set; }
        public virtual IList<ZdravstvenaSteta> ZdravstveneStete { get; set; }
        public Lekar()
        {
            ZdravstveneStete = new List<ZdravstvenaSteta>();
        }
    }
}
