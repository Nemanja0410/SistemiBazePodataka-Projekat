namespace OsiguranjApp.Entiteti
{
    public class PokretnaImovina
    {
        public virtual int PokretnaImovinaId { get; set; }
        public virtual string? Naziv { get; set; }
        public virtual string? Opis { get; set; }
        public virtual decimal? ProcenjenaVrednost { get; set; }

        public override string ToString() => Naziv ?? "";
    }
}
