namespace OsiguranjApp.Entiteti
{
    public class OblastProc
    {
        public virtual int         OblastId    { get; set; }
        public virtual Procenitelj? Procenitelj { get; set; }
        public virtual string?      Oblast      { get; set; }

        public override string ToString() => Oblast ?? "";
    }
}
