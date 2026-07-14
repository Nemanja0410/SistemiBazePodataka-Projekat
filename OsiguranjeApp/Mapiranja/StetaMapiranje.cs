using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class StetaMapiranje : ClassMap<Steta>
    {
        public StetaMapiranje()
        {
            Table("STETA");
            Id(x => x.StetaId).Column("STETA_ID").GeneratedBy.Sequence("STETA_ID_SEQ");
            Map(x => x.BrojStete).Column("BROJ_STETE").Not.Nullable();
            Map(x => x.DatumPrijave).Column("DATUM_PRIJAVE");
            Map(x => x.DatumNastanka).Column("DATUM_NASTANKA").Not.Nullable();
            Map(x => x.VrstaStete).Column("VRSTA_STETE").Not.Nullable();
            Map(x => x.OpisDogodjaja).Column("OPIS_DOGADJAJA");
            Map(x => x.Lokacija).Column("LOKACIJA");
            Map(x => x.Status).Column("STATUS");
            Map(x => x.ProcenjeniIznos).Column("PROCENJENI_IZNOS");
            Map(x => x.Valuta).Column("VALUTA");
            References(x => x.Polisa).Column("POLISA_ID").Not.Nullable();
            References(x => x.Podnosilac).Column("PODNOSILAC_ID").Not.Nullable();
            References(x => x.Agent).Column("AGENT_ID");
            HasMany(x => x.FazeObrade).KeyColumn("STETA_ID").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.ProceneSteta).KeyColumn("STETA_ID").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.OsteceniPredmeti).KeyColumn("STETA_ID").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.OstecenaLica).KeyColumn("STETA_ID").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Fotografije).KeyColumn("STETA_ID").Cascade.AllDeleteOrphan().Inverse();
        }
    }
}
