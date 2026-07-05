using System;
using System.Collections.Generic;

namespace OsiguranjApp.Entiteti
{
    public class Osoblje
    {
        public virtual int      OsobljeId         { get; set; }
        public virtual string?  Ime               { get; set; }
        public virtual string?  Prezime           { get; set; }
        public virtual string?  Jmbg              { get; set; }
        public virtual string?  Adresa            { get; set; }
        public virtual string?  Telefon           { get; set; }
        public virtual string?  Email             { get; set; }
        public virtual DateTime DatumAngazovanja  { get; set; }
        public virtual string?  Status            { get; set; }
        public virtual string?  TipOsoblja        { get; set; }

        public virtual IList<FazaObrade> FazeObrade { get; set; }

        public Osoblje()
        {
            FazeObrade = new List<FazaObrade>();
        }

        public override string ToString() => $"{Ime} {Prezime}";
    }
}
