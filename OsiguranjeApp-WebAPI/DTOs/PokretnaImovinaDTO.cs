namespace OsiguranjApp.DTOs
{
    public class PokretnaImovinaBasic
    {
        public int      PokretnaImovinaId  { get; set; }
        public string?  Naziv              { get; set; }
        public string?  Opis               { get; set; }
        public decimal? ProcenjenaVrednost { get; set; }

        public PokretnaImovinaBasic() { }
        public override string ToString() => Naziv ?? "";
    }
}
