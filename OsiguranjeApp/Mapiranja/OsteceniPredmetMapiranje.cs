using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class OsteceniPredmetMapiranje : ClassMap<OsteceniPredmet>
    {
        public OsteceniPredmetMapiranje()
        {
            Table("OSTECENI_PREDMET");
            Id(x => x.OsteceniPredmetId).Column("OSTECENI_PREDMET_ID").GeneratedBy.Sequence("OSTECENIPREDMET_ID_SEQ");
            References(x => x.Steta).Column("STETA_ID").Not.Nullable();
            Map(x => x.TipPredmeta).Column("TIP_PREDMETA");
            Map(x => x.OpisOstecenja).Column("OPIS_OSTECENJA");
            Map(x => x.ProcenjeniIznos).Column("PROCENJENI_IZNOS");
        }
    }
}
