using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class ZdravstvenaStetaMapiranje : SubclassMap<ZdravstvenaSteta>
    {
        public ZdravstvenaStetaMapiranje()
        {
            Table("ZDRAVSTVENA_STETA");
            KeyColumn("STETA_ID");
            Map(x => x.Dijagnoza).Column("DIJAGNOZA");
            Map(x => x.MedicinskaDocumentacija).Column("MEDICINSKA_DOKUMENTACIJA");
            Map(x => x.ZdravstvenaUstanova).Column("ZDRAVSTVENA_USTANOVA");
            References(x => x.Lekar).Column("LEKAR_ID");
        }
    }
}
