using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class PravnoLiceMapiranje : SubclassMap<PravnoLice>
    {
        public PravnoLiceMapiranje()
        {
            Table("PRAVNO_LICE");
            KeyColumn("KLIJENT_ID");
            Map(x => x.Pib).Column("PIB");
            Map(x => x.MaticniBroj).Column("MATICNI_BROJ");
            Map(x => x.Delatnost).Column("DELATNOST");
            HasMany(x => x.KontaktOsobe).KeyColumn("KLIJENT_ID").Cascade.AllDeleteOrphan().Inverse();
        }
    }
}
