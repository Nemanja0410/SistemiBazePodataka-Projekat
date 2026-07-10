namespace OsiguranjApp.Entiteti
{
    public class ZdravstvenaSteta : Steta
    {
        public virtual string? Dijagnoza { get; set; }
        public virtual string? MedicinskaDocumentacija { get; set; }
        public virtual string? ZdravstvenaUstanova   { get; set; }
        public virtual Lekar?  Lekar  { get; set; }
    }
}
