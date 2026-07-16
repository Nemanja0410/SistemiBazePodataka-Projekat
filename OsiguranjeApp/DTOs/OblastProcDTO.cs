namespace OsiguranjApp.DTOs
{
    public class OblastProcBasic
    {
        public int     OblastId       { get; set; }
        public int     ProceniteljId  { get; set; }
        public string? ProceniteljIme { get; set; }
        public string? Oblast         { get; set; }

        public OblastProcBasic() { }
        public override string ToString() => Oblast ?? "";
    }
}
