using System;
using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class Polisa
    {
        public virtual int      PolisaId          { get; set; }
        public virtual string?  BrojPolise        { get; set; }
        public virtual string?  TipOsiguranja     { get; set; }
        public virtual DateTime DatumZakljucenja  { get; set; }
        public virtual DateTime DatumPocetka      { get; set; }
        public virtual DateTime DatumIsteka       { get; set; }
        public virtual string?  Status            { get; set; }
        public virtual decimal  OsnovnaPremija    { get; set; }
        public virtual string?  Valuta            { get; set; }
        public virtual string?  NacinPlacanja     { get; set; }

        public virtual Agent?   Agent     { get; set; }
        public virtual Klijent? Ugovarac  { get; set; }

        public virtual IList<DodatnoPokrice> DodatnaPokrića { get; set; }
        public virtual IList<IstorijaPolise> Istorija       { get; set; }
        public virtual IList<UlogaKlijenta>  Uloge          { get; set; }
        public virtual IList<Steta>          Stete          { get; set; }

        public Polisa()
        {
            DodatnaPokrića   = new List<DodatnoPokrice>();
            Istorija         = new List<IstorijaPolise>();
            Uloge            = new List<UlogaKlijenta>();
            Stete            = new List<Steta>();
            DatumZakljucenja = DateTime.Today;
        }

        public override string ToString() => BrojPolise ?? "";
    }
}
