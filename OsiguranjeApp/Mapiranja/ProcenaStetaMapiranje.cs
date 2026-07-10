using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class ProcenaStetaMapiranje : ClassMap<ProcenaStete>
    {
        public ProcenaStetaMapiranje()
        {
            Table("PROCENA_STETE");
            Id(x => x.ProcenaId).Column("PROCENA_ID").GeneratedBy.Sequence("PROCENA_ID_SEQ");
            References(x => x.Steta).Column("STETA_ID").Not.Nullable();
            Map(x => x.DatumProc).Column("DATUM_PROCENE").Not.Nullable();
            References(x => x.Procenitelj).Column("PROCENITELJ_ID").Not.Nullable();
            Map(x => x.MetodProc).Column("METOD_PROCENE");
            Map(x => x.Nalaz).Column("NALAZ");
            Map(x => x.ProcenjeniIznos).Column("PROCENJENI_IZNOS").Not.Nullable();
            Map(x => x.Preporuka).Column("PREPORUKA");
        }
    }
}
