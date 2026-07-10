using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class SpecijalizovanoOsiguranjeMapiranje : SubclassMap<SpecijalizovanoOsiguranje>
    {
        public SpecijalizovanoOsiguranjeMapiranje()
        {
            Table("SPECIJALIZOVANO_OSIGURANJE");
            KeyColumn("POLISA_ID");
            Map(x => x.NazivSpecijalizacije).Column("NAZIV_SPECIJALIZACIJE").Not.Nullable();
            Map(x => x.OpisUslova).Column("OPIS_USLOVA");
        }
    }
}
