using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NHibernate;
using NHibernate.Linq;
using OsiguranjApp.DTOs;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp
{
    public partial class DTOManager
    {

        public static List<StetaPregled> vratiSveStete(string? vrsta = null, string? status = null)
        {
            var lista = new List<StetaPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                var q = s.Query<Steta>().AsQueryable();
                if (!string.IsNullOrEmpty(vrsta)  && vrsta  != "SVE") q = q.Where(st => st.VrstaStete == vrsta);
                if (!string.IsNullOrEmpty(status) && status != "SVE") q = q.Where(st => st.Status == status);
                foreach (var st in q.OrderByDescending(st => st.DatumPrijave))
                    lista.Add(mapStetaPregled(st));
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static StetaBasic vratiStetu(int id)
        {
            var dto = new StetaBasic();
            try
            {
                ISession s  = DataLayer.GetSession();
                Steta    st = s.Load<Steta>(id);
                dto = new StetaBasic
                {
                    StetaId = st.StetaId, BrojStete = st.BrojStete,
                    DatumPrijave = st.DatumPrijave, DatumNastanka = st.DatumNastanka,
                    PolisaId = st.Polisa?.PolisaId ?? 0, BrojPolise = st.Polisa?.BrojPolise,
                    PodnosilacId = st.Podnosilac?.KlijentId ?? 0, PodnosilacNaziv = st.Podnosilac?.Naziv,
                    VrstaStete = st.VrstaStete, OpisDogodjaja = st.OpisDogodjaja,
                    Lokacija = st.Lokacija, Status = st.Status,
                    ProcenjeniIznos = st.ProcenjeniIznos
                };
                foreach (var f in st.FazeObrade)
                    dto.FazeObrade.Add(new FazaObradeBasic
                    {
                        FazaId = f.FazaId, StetaId = st.StetaId,
                        RedniBrojFaze = f.RedniBrojFaze, NazivFaze = f.NazivFaze,
                        DatumPocetka = f.DatumPocetka, DatumZavrsetka = f.DatumZavrsetka,
                        OdgovornoLiceId  = f.OdgovornoLice?.OsobljeId,
                        OdgovornoLiceIme = f.OdgovornoLice != null
                            ? $"{f.OdgovornoLice.Ime} {f.OdgovornoLice.Prezime}" : null,
                        Odluka = f.Odluka, Dokumentacija = f.Dokumentacija,
                        Napomena = f.Napomena
                    });
                foreach (var pr in st.ProceneSteta)
                    dto.ProceneSteta.Add(new ProcenaStetaBasic
                    {
                        ProcenaId = pr.ProcenaId, StetaId = st.StetaId,
                        DatumProc = pr.DatumProc,
                        ProceniteljId  = pr.Procenitelj?.OsobljeId ?? 0,
                        ProceniteljIme = pr.Procenitelj != null ? $"{pr.Procenitelj.Ime} {pr.Procenitelj.Prezime}" : null,
                        MetodProc = pr.MetodProc, Nalaz = pr.Nalaz,
                        ProcenjeniIznos = pr.ProcenjeniIznos, Preporuka = pr.Preporuka
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return dto;
        }

        public static void dodajStetu(StetaBasic dto)
        {
            try
            {
                ISession s = DataLayer.GetSession();
                var st = new Steta
                {
                    BrojStete = dto.BrojStete, DatumPrijave = DateTime.Today,
                    DatumNastanka = dto.DatumNastanka, VrstaStete = dto.VrstaStete,
                    OpisDogodjaja = dto.OpisDogodjaja, Lokacija = dto.Lokacija,
                    Status = dto.Status ?? "PRIJAVLJENA",
                    ProcenjeniIznos = dto.ProcenjeniIznos,
                    Polisa    = s.Load<Polisa>(dto.PolisaId),
                    Podnosilac = s.Load<Klijent>(dto.PodnosilacId)
                };
                s.Save(st);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void azurirajStetu(StetaBasic dto)
        {
            try
            {
                ISession s  = DataLayer.GetSession();
                Steta    st = s.Load<Steta>(dto.StetaId);
                st.VrstaStete = dto.VrstaStete; st.OpisDogodjaja = dto.OpisDogodjaja;
                st.Lokacija = dto.Lokacija; st.Status = dto.Status;
                st.ProcenjeniIznos = dto.ProcenjeniIznos;
                st.DatumNastanka = dto.DatumNastanka;
                s.SaveOrUpdate(st);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiStetu(int id)
        {
            try
            {
                ISession s  = DataLayer.GetSession();
                Steta    st = s.Load<Steta>(id);
                s.Delete(st);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }


        public static void dodajFazuObrade(FazaObradeBasic dto)
        {
            try
            {
                ISession s = DataLayer.GetSession();
                var f = new FazaObrade
                {
                    Steta = s.Load<Steta>(dto.StetaId),
                    RedniBrojFaze = dto.RedniBrojFaze,
                    NazivFaze = dto.NazivFaze,
                    DatumPocetka = dto.DatumPocetka,
                    DatumZavrsetka = dto.DatumZavrsetka,
                    OdgovornoLice = dto.OdgovornoLiceId.HasValue
                        ? s.Load<Osoblje>(dto.OdgovornoLiceId.Value) : null,
                    Odluka = dto.Odluka,
                    Dokumentacija = dto.Dokumentacija,
                    Napomena = dto.Napomena
                };
                s.Save(f);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }


        public static List<VoziloBasic> vratiSvaVozila()
        {
            var lista = new List<VoziloBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var v in s.Query<Vozilo>().OrderBy(v => v.Registracija))
                    lista.Add(new VoziloBasic
                    {
                        VoziloId = v.VoziloId, Registracija = v.Registracija,
                        Marka = v.Marka, Model = v.Model,
                        GodinaProizvodnje = v.GodinaProizvodnje,
                        VlasnikId = v.Vlasnik?.KlijentId ?? 0, VlasnikNaziv = v.Vlasnik?.Naziv
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void dodajVozilo(VoziloBasic dto)
        {
            try
            {
                ISession s = DataLayer.GetSession();
                var v = new Vozilo
                {
                    Registracija = dto.Registracija, Marka = dto.Marka,
                    Model = dto.Model, GodinaProizvodnje = dto.GodinaProizvodnje,
                    Vlasnik = s.Load<Klijent>(dto.VlasnikId)
                };
                s.Save(v);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        private static StetaPregled mapStetaPregled(Steta st)
        {
            return new StetaPregled
            {
                StetaId = st.StetaId, BrojStete = st.BrojStete,
                DatumPrijave = st.DatumPrijave, DatumNastanka = st.DatumNastanka,
                PolisaId = st.Polisa?.PolisaId ?? 0, BrojPolise = st.Polisa?.BrojPolise,
                PodnosilacId = st.Podnosilac?.KlijentId ?? 0,
                PodnosilacNaziv = st.Podnosilac?.Naziv,
                VrstaStete = st.VrstaStete, OpisDogodjaja = st.OpisDogodjaja,
                Lokacija = st.Lokacija, Status = st.Status,
                ProcenjeniIznos = st.ProcenjeniIznos
            };
        }
    }
}
