using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class JavnaInstitucijaMapiranje : SubclassMap<JavnaInstitucija>
    {
        public JavnaInstitucijaMapiranje()
        {
            Table("JAVNA_INSTITUCIJA");
            KeyColumn("KLIJENT_ID");
            Map(x => x.Pib).Column("PIB");
            Map(x => x.MaticniBroj).Column("MATICNI_BROJ");
            Map(x => x.Delatnost).Column("DELATNOST");
            Map(x => x.NivoInstitucije).Column("NIVO_INSTITUCIJE");
            HasMany(x => x.KontaktOsobe).KeyColumn("KLIJENT_ID").Cascade.AllDeleteOrphan().Inverse();
        }
    }
}
