namespace OsiguranjApp.Entiteti
{
    public class UlogaKlijenta
    {
        public virtual int  UlogaId   { get; set; }
        public virtual Klijent? Klijent { get; set; }
        public virtual string?  ImePrezime { get; set; }
        public virtual Polisa?  Polisa  { get; set; }
        public virtual string?  TipUloge { get; set; }
    }
}
