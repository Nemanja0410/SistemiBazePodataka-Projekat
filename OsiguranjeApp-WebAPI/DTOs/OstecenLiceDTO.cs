namespace OsiguranjApp.DTOs
{
    public class OstecenLiceBasic
    {
        public int      OstecenLiceId { get; set; }
        public int      StetaId       { get; set; }
        public int?     KlijentId     { get; set; }
        public string?  KlijentNaziv  { get; set; }
        public string?  ImePrezime    { get; set; }
        public string?  OpisPovrede   { get; set; }
        public decimal? IznosNaknade  { get; set; }

        public OstecenLiceBasic() { }
        public override string ToString() => ImePrezime ?? "";
    }
}
