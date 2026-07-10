namespace OsiguranjApp.DTOs
{
    public class UsevBasic
    {
        public int      UsevId             { get; set; }
        public string?  Vrsta              { get; set; }
        public string?  Lokacija           { get; set; }
        public decimal? ProcenjenaVrednost { get; set; }

        public UsevBasic() { }
        public override string ToString() => Vrsta ?? "";
    }
}
