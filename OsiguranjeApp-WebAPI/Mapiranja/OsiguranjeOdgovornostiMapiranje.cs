using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class OsiguranjeOdgovornostiMapiranje : SubclassMap<OsiguranjeOdgovornosti>
    {
        public OsiguranjeOdgovornostiMapiranje()
        {
            Table("ODGOVORNOST_OSIGURANJE");
            KeyColumn("POLISA_ID");
            Map(x => x.VrstaOdgovornosti).Column("VRSTA_ODGOVORNOSTI");
            Map(x => x.LimitOdgovornosti).Column("LIMIT_ODGOVORNOSTI");
        }
    }
}
