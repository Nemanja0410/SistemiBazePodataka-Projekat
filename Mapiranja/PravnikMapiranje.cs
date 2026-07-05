using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class PravnikMapiranje : SubclassMap<Pravnik>
    {
        public PravnikMapiranje()
        {
            Table("PRAVNIK");
            KeyColumn("OSOBLJE_ID");
            Map(x => x.TipPravnika).Column("TIP_PRAVNIKA");
            Map(x => x.BarBroj).Column("BAR_BROJ");
        }
    }
}
