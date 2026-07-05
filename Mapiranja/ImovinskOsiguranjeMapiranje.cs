using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class ImovinskOsiguranjeMapiranje : SubclassMap<ImovinskOsiguranje>
    {
        public ImovinskOsiguranjeMapiranje()
        {
            Table("IMOVINSKO_OSIGURANJE");
            KeyColumn("POLISA_ID");
            Map(x => x.VrsteRizika).Column("VRSTE_RIZIKA");
            HasManyToMany(x => x.Nekretnine)
                .Table("POLISA_NEKRETNINA")
                .ParentKeyColumn("POLISA_ID")
                .ChildKeyColumn("NEKRETNINA_ID")
                .Cascade.None();
        }
    }
}
