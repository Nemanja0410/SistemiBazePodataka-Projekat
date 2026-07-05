using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class ZivotnoOsiguranjeMapiranje : SubclassMap<ZivotnoOsiguranje>
    {
        public ZivotnoOsiguranjeMapiranje()
        {
            Table("ZIVOTNO_OSIGURANJE");
            KeyColumn("POLISA_ID");
            Map(x => x.SumaOsiguranja).Column("SUMA_OSIGURANJA");
            Map(x => x.TipIsplate).Column("TIP_ISPLATE");
            HasMany(x => x.KorisniciIsplate).KeyColumn("POLISA_ID").Cascade.AllDeleteOrphan().Inverse();
        }
    }
}
