using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class OblastProcMapiranje : ClassMap<OblastProc>
    {
        public OblastProcMapiranje()
        {
            Table("OBLAST_PROCENE");
            Id(x => x.OblastId).Column("OBLAST_ID").GeneratedBy.Sequence("OBLAST_ID_SEQ");
            References(x => x.Procenitelj).Column("OSOBLJE_ID");
            Map(x => x.Oblast).Column("OBLAST");
        }
    }
}
