using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class FotografijaMapiranje : ClassMap<Fotografija>
    {
        public FotografijaMapiranje()
        {
            Table("FOTOGRAFIJA");
            Id(x => x.FotografijaId).Column("FOTOGRAFIJA_ID").GeneratedBy.Sequence("FOTOGRAFIJA_ID_SEQ");
            References(x => x.Steta).Column("STETA_ID").Not.Nullable();
            Map(x => x.Putanja).Column("PUTANJA").Not.Nullable();
            Map(x => x.Opis).Column("OPIS");
            Map(x => x.DatumDodavanja).Column("DATUM_DODAVANJA");
        }
    }
}
