using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class AgentMapiranje : SubclassMap<Agent>
    {
        public AgentMapiranje()
        {
            Table("AGENT");
            KeyColumn("OSOBLJE_ID");
            Map(x => x.TipAgenta).Column("TIP_AGENTA");
            Map(x => x.Licenca).Column("LICENCA");
            Map(x => x.RegionRada).Column("REGION_RADA");
            Map(x => x.ProvizijaProcenat).Column("PROVIZIJA_PROCENAT");
            HasMany(x => x.Polise).KeyColumn("AGENT_ID").Cascade.None();
            HasMany(x => x.Stete).KeyColumn("AGENT_ID").Cascade.None();
        }
    }
}
