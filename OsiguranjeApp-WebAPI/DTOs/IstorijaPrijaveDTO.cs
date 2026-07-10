using System;

namespace OsiguranjApp.DTOs
{
    public class IstorijaPrijaveBasic
    {
        public int      IstorijaPrijaveId    { get; set; }
        public int?     NalogId              { get; set; }
        public string?  KorisnickoImePokusaj { get; set; }
        public DateTime VremePokusaja        { get; set; }
        public bool     Uspesno              { get; set; }
        public string?  Razlog               { get; set; }

        public IstorijaPrijaveBasic() { }
    }
}
