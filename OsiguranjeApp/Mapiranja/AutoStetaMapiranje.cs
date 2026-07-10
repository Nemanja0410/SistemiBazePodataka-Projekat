using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class AutoStetaMapiranje : SubclassMap<AutoSteta>
    {
        public AutoStetaMapiranje()
        {
            Table("AUTO_STETA");
            KeyColumn("STETA_ID");
            Map(x => x.ZapisnikPolicije).Column("ZAPISNIK_POLICIJE");
            Map(x => x.Servis).Column("SERVIS_ID");
            References(x => x.Vozilo).Column("VOZILO_ID");
        }
    }
}
