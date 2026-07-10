namespace OsiguranjApp.DTOs
{
    public class ZivotinjaBasic
    {
        public int      ZivotinjaId        { get; set; }
        public string?  Vrsta              { get; set; }
        public string?  Lokacija           { get; set; }
        public decimal? ProcenjenaVrednost { get; set; }

        public ZivotinjaBasic() { }
        public override string ToString() => Vrsta ?? "";
    }
}
