using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class PolisaMapiranje : ClassMap<Polisa>
    {
        public PolisaMapiranje()
        {
            Table("POLISA");
            Id(x => x.PolisaId).Column("POLISA_ID").GeneratedBy.Sequence("POLISA_ID_SEQ");
            Map(x => x.BrojPolise).Column("BROJ_POLISE").Not.Nullable();
            Map(x => x.TipOsiguranja).Column("TIP_OSIGURANJA").Not.Nullable();
            Map(x => x.DatumZakljucenja).Column("DATUM_ZAKLJUCENJA");
            Map(x => x.DatumPocetka).Column("DATUM_POCETKA").Not.Nullable();
            Map(x => x.DatumIsteka).Column("DATUM_ISTEKA").Not.Nullable();
            Map(x => x.Status).Column("STATUS");
            Map(x => x.OsnovnaPremija).Column("OSNOVNA_PREMIJA").Not.Nullable();
            Map(x => x.Valuta).Column("VALUTA");
            Map(x => x.NacinPlacanja).Column("NACIN_PLACANJA");
            References(x => x.Agent).Column("AGENT_ID");
            References(x => x.Ugovarac).Column("UGOVARAC_ID").Not.Nullable();
            HasMany(x => x.DodatnaPokrića).KeyColumn("POLISA_ID").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Istorija).KeyColumn("POLISA_ID").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Uloge).KeyColumn("POLISA_ID").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Stete).KeyColumn("POLISA_ID").Cascade.None();
        }
    }
}
