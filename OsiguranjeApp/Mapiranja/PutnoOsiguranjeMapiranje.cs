using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class PutnoOsiguranjeMapiranje : SubclassMap<PutnoOsiguranje>
    {
        public PutnoOsiguranjeMapiranje()
        {
            Table("PUTOVANJE");
            KeyColumn("POLISA_ID");
            Map(x => x.Destinacije).Column("DESTINACIJE");
            Map(x => x.DatumPolaska).Column("DATUM_POLASKA");
            Map(x => x.DatumPovratka).Column("DATUM_POVRATKA");
            HasManyToMany(x => x.OsiguranaLica)
                .Table("PUTOVANJE_LICE")
                .ParentKeyColumn("POLISA_ID")
                .ChildKeyColumn("KLIJENT_ID")
                .Cascade.None();
        }
    }
}
