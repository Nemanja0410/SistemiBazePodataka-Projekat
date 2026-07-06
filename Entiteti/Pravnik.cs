namespace OsiguranjApp.Entiteti
{
    public class Pravnik : Osoblje
    {
        public virtual string? TipPravnika { get; set; }
        public virtual string? BarBroj { get; set; }
    }
}
