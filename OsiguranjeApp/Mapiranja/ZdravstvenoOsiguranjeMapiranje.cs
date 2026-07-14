using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class ZdravstvenoOsiguranjeMapiranje : SubclassMap<ZdravstvenoOsiguranje>
    {
        public ZdravstvenoOsiguranjeMapiranje()
        {
            Table("ZDRAVSTVENO_OSIGURANJE");
            KeyColumn("POLISA_ID");
            Map(x => x.MrezaUstanova).Column("MREZA_USTANOVA");
            Map(x => x.LimitSpecijalista).Column("LIMIT_SPECIJALISTA");
            Map(x => x.LimitStomatologa).Column("LIMIT_STOMATOLOGA");
            Map(x => x.LimitBolnickih).Column("LIMIT_BOLNICKIH");
            Map(x => x.LimitBolnickiDan).Column("LIMIT_BOLNICKI_DAN");
            Map(x => x.Pokrica).Column("POKRICA");
        }
    }
}
