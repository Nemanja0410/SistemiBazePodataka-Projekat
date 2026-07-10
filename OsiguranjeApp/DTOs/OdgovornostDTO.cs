namespace OsiguranjApp.DTOs
{
    public class OdgovornostPregled : PolisaPregled
    {
        public string?  VrstaOdgovornosti { get; set; }
        public decimal? LimitOdgovornosti { get; set; }

        public OdgovornostPregled() { }
    }
}
