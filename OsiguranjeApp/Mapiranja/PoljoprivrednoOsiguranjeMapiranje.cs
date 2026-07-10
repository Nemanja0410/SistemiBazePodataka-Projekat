using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class PoljoprivrednoOsiguranjeMapiranje : SubclassMap<PoljoprivrednoOsiguranje>
    {
        public PoljoprivrednoOsiguranjeMapiranje()
        {
            Table("POLJOPRIVREDNO_OSIGURANJE");
            KeyColumn("POLISA_ID");
            HasManyToMany(x => x.Usevi)
                .Table("POLJOPRIVREDNO_USEV")
                .ParentKeyColumn("POLISA_ID")
                .ChildKeyColumn("USEV_ID")
                .Cascade.None();
            HasManyToMany(x => x.Zivotinje)
                .Table("POLJOPRIVREDNO_ZIVOTINJA")
                .ParentKeyColumn("POLISA_ID")
                .ChildKeyColumn("ZIVOTINJA_ID")
                .Cascade.None();
        }
    }
}
