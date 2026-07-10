using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class Nekretnina
    {
        public virtual int NekretninaId { get; set; }
        public virtual string? Adresa { get; set; }
        public virtual string? TipObjekta { get; set; }
        public virtual decimal Povrsina { get; set; }
        public virtual int GodinaIzgradnje{ get; set; }
        public virtual decimal ProcenjenaVrednost { get; set; }
        public virtual IList<ImovinskOsiguranje> Osiguranja { get; set; }
        public Nekretnina()
        {
            Osiguranja = new List<ImovinskOsiguranje>();
        }
        public override string ToString() => Adresa ?? "";
    }
}
