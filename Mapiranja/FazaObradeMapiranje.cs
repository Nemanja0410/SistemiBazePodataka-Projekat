using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class FazaObradeMapiranje : ClassMap<FazaObrade>
    {
        public FazaObradeMapiranje()
        {
            Table("FAZA_OBRADE");
            Id(x => x.FazaId).Column("FAZA_ID").GeneratedBy.Sequence("FAZA_ID_SEQ");
            References(x => x.Steta).Column("STETA_ID").Not.Nullable();
            Map(x => x.RedniBrojFaze).Column("REDNI_BROJ_FAZE");
            Map(x => x.NazivFaze).Column("NAZIV_FAZE").Not.Nullable();
            Map(x => x.DatumPocetka).Column("DATUM_POCETKA").Not.Nullable();
            Map(x => x.DatumZavrsetka).Column("DATUM_ZAVRSETKA");
            References(x => x.OdgovornoLice).Column("ODGOVORNO_LICE_ID");
            Map(x => x.Odluka).Column("ODLUKA");
            Map(x => x.Dokumentacija).Column("DOKUMENTACIJA");
            Map(x => x.Napomena).Column("NAPOMENA");
        }
    }
}
