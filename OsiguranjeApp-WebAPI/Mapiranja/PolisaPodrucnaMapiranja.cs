using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class DodatnoPokrMapiranje : ClassMap<DodatnoPokrice>
    {
        public DodatnoPokrMapiranje()
        {
            Table("DODATNO_POKRICE");
            Id(x => x.PokriceId).Column("POKRICE_ID").GeneratedBy.Sequence("POKRICE_ID_SEQ");
            References(x => x.Polisa).Column("POLISA_ID").Not.Nullable();
            Map(x => x.Naziv).Column("NAZIV").Not.Nullable();
            Map(x => x.Opis).Column("OPIS");
            Map(x => x.LimitPokrića).Column("LIMIT_POKRICA");
            Map(x => x.Fransiza).Column("FRANSIZA");
            Map(x => x.DodatnaPremija).Column("DODATNA_PREMIJA");
        }
    }

    public class UlogaKlijentaMapiranje : ClassMap<UlogaKlijenta>
    {
        public UlogaKlijentaMapiranje()
        {
            Table("ULOGA_KLIJENTA");
            Id(x => x.UlogaId).Column("ULOGA_ID").GeneratedBy.Sequence("ULOGA_ID_SEQ");
            References(x => x.Klijent).Column("KLIJENT_ID");
            Map(x => x.ImePrezime).Column("IME_PREZIME");
            References(x => x.Polisa).Column("POLISA_ID").Not.Nullable();
            Map(x => x.TipUloge).Column("TIP_ULOGE").Not.Nullable();
        }
    }

    public class KorisnikIsplateMapiranje : ClassMap<KorisnikIsplate>
    {
        public KorisnikIsplateMapiranje()
        {
            Table("KORISNIK_ISPLATE");
            Id(x => x.KorisnikId).Column("KORISNIK_ID").GeneratedBy.Sequence("KORISNIK_ID_SEQ");
            References(x => x.Polisa).Column("POLISA_ID").Not.Nullable();
            References(x => x.Klijent).Column("KLIJENT_ID");
            Map(x => x.ImePrezime).Column("IME_PREZIME");
            Map(x => x.ProcenatUdela).Column("PROCENAT_UDELA");
        }
    }

    public class IstorijaPoliseMapiranje : ClassMap<IstorijaPolise>
    {
        public IstorijaPoliseMapiranje()
        {
            Table("ISTORIJA_POLISE");
            Id(x => x.IstorijaId).Column("ISTORIJA_ID").GeneratedBy.Sequence("ISTORIJA_ID_SEQ");
            References(x => x.Polisa).Column("POLISA_ID").Not.Nullable();
            Map(x => x.TipPromene).Column("TIP_PROMENE").Not.Nullable();
            Map(x => x.DatumPromene).Column("DATUM_PROMENE");
            Map(x => x.Opis).Column("OPIS");
            References(x => x.Korisnik).Column("KORISNIK_OSOBLJE_ID");
        }
    }
}
