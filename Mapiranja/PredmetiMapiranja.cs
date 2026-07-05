using FluentNHibernate.Mapping;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp.Mapiranja
{
    public class VoziloMapiranje : ClassMap<Vozilo>
    {
        public VoziloMapiranje()
        {
            Table("VOZILO");
            Id(x => x.VoziloId).Column("VOZILO_ID").GeneratedBy.Sequence("VOZILO_ID_SEQ");
            Map(x => x.Registracija).Column("REGISTRACIJA").Not.Nullable();
            Map(x => x.Marka).Column("MARKA").Not.Nullable();
            Map(x => x.Model).Column("MODEL").Not.Nullable();
            Map(x => x.GodinaProizvodnje).Column("GODINA_PROIZVODNJE");
            References(x => x.Vlasnik).Column("VLASNIK_ID").Not.Nullable();
        }
    }

    public class NekretninaMapiranje : ClassMap<Nekretnina>
    {
        public NekretninaMapiranje()
        {
            Table("NEKRETNINA");
            Id(x => x.NekretninaId).Column("NEKRETNINA_ID").GeneratedBy.Sequence("NEKRETNINA_ID_SEQ");
            Map(x => x.Adresa).Column("ADRESA").Not.Nullable();
            Map(x => x.TipObjekta).Column("TIP_OBJEKTA").Not.Nullable();
            Map(x => x.Povrsina).Column("POVRSINA");
            Map(x => x.GodinaIzgradnje).Column("GODINA_IZGRADNJE");
            Map(x => x.ProcenjenaVrednost).Column("PROCENJENA_VREDNOST");
        }
    }
}
