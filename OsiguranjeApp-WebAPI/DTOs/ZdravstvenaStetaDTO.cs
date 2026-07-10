namespace OsiguranjApp.DTOs
{
    public class ZdravstvenaStetaPregled : StetaPregled
    {
        public string? Dijagnoza                { get; set; }
        public string? MedicinskaDocumentacija   { get; set; }
        public string? ZdravstvenaUstanova       { get; set; }
        public int?    LekarId                   { get; set; }
        public string? LekarIme                  { get; set; }

        public ZdravstvenaStetaPregled() { }
    }
}
