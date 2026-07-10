using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class OstecenLiceMapiranje : ClassMap<OstecenLice>
    {
        public OstecenLiceMapiranje()
        {
            Table("OSTECENO_LICE");
            Id(x => x.OstecenLiceId).Column("OSTECENO_LICE_ID").GeneratedBy.Sequence("OSTECENOLICE_ID_SEQ");
            References(x => x.Steta).Column("STETA_ID").Not.Nullable();
            References(x => x.Klijent).Column("KLIJENT_ID");
            Map(x => x.ImePrezime).Column("IME_PREZIME");
            Map(x => x.OpisPovrede).Column("OPIS_POVREDE");
            Map(x => x.IznosNaknade).Column("IZNOS_NAKNADE");
        }
    }
}
