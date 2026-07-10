namespace OsiguranjApp.Entiteti
{
    public class Usev
    {
        public virtual int UsevId { get; set; }
        public virtual string? Vrsta { get; set; }
        public virtual string? Lokacija { get; set; }
        public virtual decimal? ProcenjenaVrednost { get; set; }

        public override string ToString() => Vrsta ?? "";
    }
}
