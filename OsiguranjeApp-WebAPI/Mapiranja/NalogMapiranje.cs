using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class NalogMapiranje : ClassMap<Nalog>
    {
        public NalogMapiranje()
        {
            Table("NALOG");
            Id(x => x.NalogId).Column("NALOG_ID").GeneratedBy.Sequence("NALOG_ID_SEQ");
            Map(x => x.KorisnickoIme).Column("KORISNICKO_IME").Not.Nullable();
            Map(x => x.LozinkaHash).Column("LOZINKA_HASH").Not.Nullable();
            Map(x => x.LozinkaSalt).Column("LOZINKA_SALT").Not.Nullable();
            Map(x => x.LozinkaIteracije).Column("LOZINKA_ITERACIJE");
            References(x => x.Osoblje).Column("OSOBLJE_ID");
            Map(x => x.Uloga).Column("ULOGA");
            Map(x => x.StatusNaloga).Column("STATUS_NALOGA");
            Map(x => x.NeuspesnihPrijava).Column("NEUSPESNIH_PRIJAVA");
            Map(x => x.MoraPromenitiLozinku).Column("MORA_PROMENITI_LOZINKU");
            Map(x => x.DatumRegistracije).Column("DATUM_REGISTRACIJE");
            Map(x => x.DatumOdobrenja).Column("DATUM_ODOBRENJA");
            Map(x => x.ZadnjaPrijava).Column("ZADNJA_PRIJAVA");
            References(x => x.OdobrioNalog).Column("ODOBRIO_NALOG_ID");
        }
    }
}
