using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class IstorijaPrijaveMapiranje : ClassMap<IstorijaPrijave>
    {
        public IstorijaPrijaveMapiranje()
        {
            Table("ISTORIJA_PRIJAVE");
            Id(x => x.IstorijaPrijaveId).Column("ISTORIJA_PRIJAVE_ID").GeneratedBy.Sequence("ISTORIJA_PRIJAVE_ID_SEQ");
            References(x => x.Nalog).Column("NALOG_ID");
            Map(x => x.KorisnickoImePokusaj).Column("KORISNICKO_IME_POKUSAJ").Not.Nullable();
            Map(x => x.VremePokusaja).Column("VREME_POKUSAJA");
            Map(x => x.Uspesno).Column("USPESNO");
            Map(x => x.Razlog).Column("RAZLOG");
        }
    }
}
