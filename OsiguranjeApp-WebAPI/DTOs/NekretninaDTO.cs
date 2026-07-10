namespace OsiguranjApp.DTOs
{
    public class NekretninaBasic
    {
        public int     NekretninaId       { get; set; }
        public string? Adresa             { get; set; }
        public string? TipObjekta         { get; set; }
        public decimal Povrsina           { get; set; }
        public int     GodinaIzgradnje    { get; set; }
        public decimal ProcenjenaVrednost { get; set; }

        public NekretninaBasic() { }
        public override string ToString() => Adresa ?? "";
    }
}
