using System;

namespace OsiguranjApp.DTOs
{
    public class NalogPregled
    {
        public int       NalogId               { get; set; }
        public string?   KorisnickoIme         { get; set; }
        public int?      OsobljeId             { get; set; }
        public string?   ImeOsoblja            { get; set; }
        public string?   PrezimeOsoblja        { get; set; }
        public string?   TipOsoblja            { get; set; }
        public string?   Uloga                 { get; set; }
        public string?   StatusNaloga          { get; set; }
        public bool      MoraPromenitiLozinku  { get; set; }
        public int       NeuspesnihPrijava     { get; set; }
        public DateTime  DatumRegistracije     { get; set; }
        public DateTime? DatumOdobrenja        { get; set; }
        public DateTime? ZadnjaPrijava         { get; set; }

        public NalogPregled() { }
        public override string ToString() =>
            ImeOsoblja != null ? $"{KorisnickoIme} ({ImeOsoblja} {PrezimeOsoblja})" : KorisnickoIme ?? "";
    }

    public class RegistracijaZahtev
    {
        public int     OsobljeId     { get; set; }
        public string? KorisnickoIme { get; set; }
        public string? Lozinka       { get; set; }
    }

    public class PrijavaRezultat
    {
        public bool           Uspesno { get; set; }
        public string?        Poruka  { get; set; }
        public NalogPregled?  Nalog   { get; set; }
    }
}
