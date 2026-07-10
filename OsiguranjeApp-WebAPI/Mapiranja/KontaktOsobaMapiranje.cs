using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class KontaktOsobaMapiranje : ClassMap<KontaktOsoba>
    {
        public KontaktOsobaMapiranje()
        {
            Table("KONTAKT_OSOBA");
            Id(x => x.KontaktId).Column("KONTAKT_ID").GeneratedBy.Sequence("KONTAKT_ID_SEQ");
            References(x => x.Klijent).Column("KLIJENT_ID").Not.Nullable();
            Map(x => x.Ime).Column("IME");
            Map(x => x.Prezime).Column("PREZIME");
            Map(x => x.Telefon).Column("TELEFON");
            Map(x => x.Email).Column("EMAIL");
            Map(x => x.Funkcija).Column("FUNKCIJA");
        }
    }
}
