namespace OsiguranjApp.Entiteti
{
    public class KontaktOsoba
    {
        public virtual int KontaktId { get; set; }
        public virtual Klijent? Klijent{ get; set; }
        public virtual string? Ime{ get; set; }
        public virtual string? Prezime { get; set; }
        public virtual string? Telefon { get; set; }
        public virtual string? Email { get; set; }
        public virtual string? Funkcija{ get; set; }
        public override string ToString() => $"{Ime} {Prezime}";
    }
}
