using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class LekarMapiranje : SubclassMap<Lekar>
    {
        public LekarMapiranje()
        {
            Table("LEKAR");
            KeyColumn("OSOBLJE_ID");
            Map(x => x.Specijalizacija).Column("SPECIJALIZACIJA");
            Map(x => x.LicencaBroj).Column("LICENCA_BROJ");
            HasMany(x => x.ZdravstveneStete).KeyColumn("LEKAR_ID").Cascade.None();
        }
    }
}
