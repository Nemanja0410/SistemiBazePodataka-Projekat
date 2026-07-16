namespace OsiguranjApp.Entiteti
{
    public class UlogaKlijenta
    {
        public virtual int  UlogaId   { get; set; }
        public virtual Klijent? Klijent { get; set; }
        public virtual Polisa?  Polisa  { get; set; }
        public virtual string?  TipUloge { get; set; }
        // Osiguranik moze biti registrovan klijent (Klijent) ili slobodan unos imena (ImePrezime)
        // kad ta osoba nije klijent kompanije - isto kao OstecenLice kod steta.
        public virtual string? ImePrezime { get; set; }
    }
}
