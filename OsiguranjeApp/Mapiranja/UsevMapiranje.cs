using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class UsevMapiranje : ClassMap<Usev>
    {
        public UsevMapiranje()
        {
            Table("USEV");
            Id(x => x.UsevId).Column("USEV_ID").GeneratedBy.Sequence("USEV_ID_SEQ");
            Map(x => x.Vrsta).Column("VRSTA").Not.Nullable();
            Map(x => x.Lokacija).Column("LOKACIJA");
            Map(x => x.ProcenjenaVrednost).Column("PROCENJENA_VREDNOST");
        }
    }
}
