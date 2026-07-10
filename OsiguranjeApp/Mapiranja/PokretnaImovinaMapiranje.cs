using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class PokretnaImovinaMapiranje : ClassMap<PokretnaImovina>
    {
        public PokretnaImovinaMapiranje()
        {
            Table("POKRETNA_IMOVINA");
            Id(x => x.PokretnaImovinaId).Column("POKRETNA_IMOVINA_ID").GeneratedBy.Sequence("POKRETNAIMOVINA_ID_SEQ");
            Map(x => x.Naziv).Column("NAZIV").Not.Nullable();
            Map(x => x.Opis).Column("OPIS");
            Map(x => x.ProcenjenaVrednost).Column("PROCENJENA_VREDNOST");
        }
    }
}
