using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class AutoOsiguranjeMapiranje : SubclassMap<AutoOsiguranje>
    {
        public AutoOsiguranjeMapiranje()
        {
            Table("AUTO_OSIGURANJE");
            KeyColumn("POLISA_ID");
            References(x => x.Vozilo).Column("VOZILO_ID");
            Map(x => x.BonusMalusKlasa).Column("BONUS_MALUS_KLASA");
            Map(x => x.TeritorijanoVazenje).Column("TERITORIJALNO_VAZENJE");
            HasManyToMany(x => x.Vozaci)
                .Table("VOZAC_POLISE")
                .ParentKeyColumn("POLISA_ID")
                .ChildKeyColumn("KLIJENT_ID")
                .Cascade.None();
        }
    }
}
