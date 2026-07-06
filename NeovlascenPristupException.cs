using System;

namespace OsiguranjApp
{
    public class NeovlascenPristupException : Exception
    {
        public NeovlascenPristupException(string poruka = "Nemate ovlašćenje za ovu akciju.") : base(poruka) { }
    }
}
