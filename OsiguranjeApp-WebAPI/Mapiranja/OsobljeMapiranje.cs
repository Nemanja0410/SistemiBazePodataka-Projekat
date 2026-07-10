using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class OsobljeMapiranje : ClassMap<Osoblje>
    {
        public OsobljeMapiranje()
        {
            Table("OSOBLJE");
            Id(x => x.OsobljeId).Column("OSOBLJE_ID").GeneratedBy.Sequence("OSOBLJE_ID_SEQ");
            Map(x => x.Ime).Column("IME").Not.Nullable();
            Map(x => x.Prezime).Column("PREZIME").Not.Nullable();
            Map(x => x.Jmbg).Column("JMBG");
            Map(x => x.Adresa).Column("ADRESA");
            Map(x => x.Telefon).Column("TELEFON");
            Map(x => x.Email).Column("EMAIL");
            Map(x => x.DatumAngazovanja).Column("DATUM_ANGAZOVANJA");
            Map(x => x.Status).Column("STATUS");
            Map(x => x.TipOsoblja).Column("TIP_OSOBLJA");
            HasMany(x => x.FazeObrade).KeyColumn("ODGOVORNO_LICE_ID").Cascade.None();
        }
    }
}
