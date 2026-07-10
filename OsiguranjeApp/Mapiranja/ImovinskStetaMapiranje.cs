using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class ImovinskStetaMapiranje : SubclassMap<ImovinskSteta>
    {
        public ImovinskStetaMapiranje()
        {
            Table("IMOVINSKA_STETA");
            KeyColumn("STETA_ID");
            Map(x => x.ProcenaOstecenja).Column("PROCENA_OSTECENJA");
            Map(x => x.IzvodjacSanacije).Column("IZVODJAC_SANACIJE");
        }
    }
}
