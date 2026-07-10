namespace OsiguranjApp.Entiteti
{
    public class OsiguranjeOdgovornosti : Polisa
    {
        public virtual string? VrstaOdgovornosti { get; set; }
        public virtual decimal? LimitOdgovornosti { get; set; }
    }
}
