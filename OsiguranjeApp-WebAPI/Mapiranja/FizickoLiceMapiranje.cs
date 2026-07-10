using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class FizickoLiceMapiranje : SubclassMap<FizickoLice>
    {
        public FizickoLiceMapiranje()
        {
            Table("FIZICKO_LICE");
            KeyColumn("KLIJENT_ID");
            Map(x => x.Jmbg).Column("JMBG");
            Map(x => x.DatumRodjenja).Column("DATUM_RODJENJA");
            Map(x => x.Zanimanje).Column("ZANIMANJE");
        }
    }
}
