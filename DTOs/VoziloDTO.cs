namespace OsiguranjApp.DTOs
{
    public class VoziloBasic
    {
        public int    VoziloId          { get; set; }
        public string? Registracija      { get; set; }
        public string? Marka             { get; set; }
        public string? Model             { get; set; }
        public int     GodinaProizvodnje { get; set; }
        public int     VlasnikId         { get; set; }
        public string? VlasnikNaziv      { get; set; }

        public VoziloBasic() { }
        public override string ToString() => $"{Registracija} – {Marka} {Model}";
    }
}
