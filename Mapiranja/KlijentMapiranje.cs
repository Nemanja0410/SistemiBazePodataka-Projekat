using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class KlijentMapiranje : ClassMap<Klijent>
    {
        public KlijentMapiranje()
        {
            Table("KLIJENT");
            Id(x => x.KlijentId).Column("KLIJENT_ID").GeneratedBy.Sequence("KLIJENT_ID_SEQ");
            Map(x => x.Naziv).Column("NAZIV").Not.Nullable();
            Map(x => x.Adresa).Column("ADRESA");
            Map(x => x.Telefon).Column("TELEFON");
            Map(x => x.Email).Column("EMAIL");
            Map(x => x.Status).Column("STATUS");
            Map(x => x.DatumRegistracije).Column("DATUM_REGISTRACIJE");
            Map(x => x.TipKlijenta).Column("TIP_KLIJENTA").Not.Nullable();
            HasMany(x => x.Polise).KeyColumn("UGOVARAC_ID").Cascade.None();
            HasMany(x => x.Stete).KeyColumn("PODNOSILAC_ID").Cascade.None();
        }
    }
}
