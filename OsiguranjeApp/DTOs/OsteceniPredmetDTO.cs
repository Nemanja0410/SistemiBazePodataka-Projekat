namespace OsiguranjApp.DTOs
{
    public class OsteceniPredmetBasic
    {
        public int      OsteceniPredmetId { get; set; }
        public int      StetaId           { get; set; }
        public string?  TipPredmeta       { get; set; }
        public string?  OpisOstecenja     { get; set; }
        public decimal? ProcenjeniIznos   { get; set; }

        public OsteceniPredmetBasic() { }
        public override string ToString() => TipPredmeta ?? "";
    }
}
