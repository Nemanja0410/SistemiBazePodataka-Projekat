using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class ProceniteljMapiranje : SubclassMap<Procenitelj>
    {
        public ProceniteljMapiranje()
        {
            Table("PROCENITELJ");
            KeyColumn("OSOBLJE_ID");
            Map(x => x.BrojLicence).Column("BROJ_LICENCE");
            HasMany(x => x.OblasiProc).KeyColumn("OSOBLJE_ID").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.ProceneSteta).KeyColumn("PROCENITELJ_ID").Cascade.None();
        }
    }
}
