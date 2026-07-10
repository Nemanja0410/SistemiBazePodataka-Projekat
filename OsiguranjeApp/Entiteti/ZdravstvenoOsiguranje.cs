namespace OsiguranjApp.Entiteti
{
    public class ZdravstvenoOsiguranje : Polisa
    {
        public virtual string? MrezaUstanova { get; set; }
        public virtual decimal LimitSpecijalista { get; set; }
        public virtual decimal LimitStomatologa { get; set; }
        public virtual decimal LimitBolnickih  { get; set; }
        public virtual decimal LimitBolnickiDan  { get; set; }
    }
}
