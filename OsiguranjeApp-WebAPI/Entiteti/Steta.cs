using System;
using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class Steta
    {
        public virtual int  StetaId { get; set; }
        public virtual string?  BrojStete { get; set; }
        public virtual DateTime DatumPrijave { get; set; }
        public virtual DateTime DatumNastanka { get; set; }
        public virtual Polisa?  Polisa  { get; set; }
        public virtual Klijent? Podnosilac { get; set; }
        public virtual string?  VrstaStete { get; set; }
        public virtual string?  OpisDogodjaja  { get; set; }
        public virtual string?  Lokacija{ get; set; }
        public virtual string?  Status{ get; set; }
        public virtual decimal? ProcenjeniIznos { get; set; }
        public virtual string?  Valuta { get; set; }
        public virtual Agent? Agent  { get; set; }

        public virtual IList<FazaObrade>  FazeObrade  { get; set; }
        public virtual IList<ProcenaStete> ProceneSteta   { get; set; }
        public virtual IList<OsteceniPredmet> OsteceniPredmeti { get; set; }
        public virtual IList<OstecenLice>  OstecenaLica  { get; set; }
        public virtual IList<Fotografija>  Fotografije   { get; set; }
        public Steta()
        {
            FazeObrade       = new List<FazaObrade>();
            ProceneSteta     = new List<ProcenaStete>();
            OsteceniPredmeti = new List<OsteceniPredmet>();
            OstecenaLica     = new List<OstecenLice>();
            Fotografije      = new List<Fotografija>();
            DatumPrijave     = DateTime.Today;
        }
        public override string ToString() => BrojStete ?? "";
    }
}
