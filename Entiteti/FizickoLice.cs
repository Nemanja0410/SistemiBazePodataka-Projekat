using System;

namespace OsiguranjApp.Entiteti
{
    public class FizickoLice : Klijent
    {
        public virtual string? Jmbg { get; set; }
        public virtual DateTime? DatumRodjenja { get; set; }
        public virtual string? Zanimanje { get; set; }
    }
}
