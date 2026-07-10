using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class ZivotinjaMapiranje : ClassMap<Zivotinja>
    {
        public ZivotinjaMapiranje()
        {
            Table("ZIVOTINJA");
            Id(x => x.ZivotinjaId).Column("ZIVOTINJA_ID").GeneratedBy.Sequence("ZIVOTINJA_ID_SEQ");
            Map(x => x.Vrsta).Column("VRSTA").Not.Nullable();
            Map(x => x.Lokacija).Column("LOKACIJA");
            Map(x => x.ProcenjenaVrednost).Column("PROCENJENA_VREDNOST");
        }
    }
}
