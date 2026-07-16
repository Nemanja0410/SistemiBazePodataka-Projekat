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
    // Tip-specificna polja stete (Auto/Zdravstvena/Imovinska) + Ostecena lica, Osteceni
    // predmeti i Procene stete, koji se upisuju direktno kroz Prijavi/Izmeni stetu.
    public partial class DTOManager
    {
        private static Steta popuniBazuStete(Steta st, StetaPregled dto, ISession s)
        {
            st.BrojStete = dto.BrojStete;
            if (st.StetaId == 0) st.DatumPrijave = DateTime.Today; // samo pri kreiranju, ne pri izmeni
            st.DatumNastanka = dto.DatumNastanka;
            st.Status = dto.Status ?? "PRIJAVLJENA";
            st.OpisDogodjaja = dto.OpisDogodjaja;
            st.Lokacija = dto.Lokacija;
            st.ProcenjeniIznos = dto.ProcenjeniIznos;
            st.Valuta = dto.Valuta ?? "RSD";
            st.Polisa = s.Load<Polisa>(dto.PolisaId);
            st.Podnosilac = s.Load<Klijent>(dto.PodnosilacId);
            return st;
        }

        // ---------- vratiStetuDetaljno: puni tip-specificni DTO za citanje/izmenu ----------

        public static object vratiStetuDetaljno(int id)
        {
            try
            {
                ISession s  = DataLayer.GetSession();
                Steta    st = s.Load<Steta>(id);
                object dto = st switch
                {
                    AutoSteta a => new AutoStetaBasic
                    {
                        ZapisnikPolicije = a.ZapisnikPolicije, Servis = a.Servis,
                        VoziloId = a.Vozilo?.VoziloId, VoziloOpis = a.Vozilo?.ToString()
                    },
                    ZdravstvenaSteta z => new ZdravstvenaStetaBasic
                    {
                        Dijagnoza = z.Dijagnoza, MedicinskaDocumentacija = z.MedicinskaDocumentacija,
                        ZdravstvenaUstanova = z.ZdravstvenaUstanova,
                        LekarId = z.Lekar?.OsobljeId, LekarIme = z.Lekar != null ? $"{z.Lekar.Ime} {z.Lekar.Prezime}" : null
                    },
                    ImovinskSteta im => new ImovinskStetaBasic
                    {
                        ProcenaOstecenja = im.ProcenaOstecenja, IzvodjacSanacije = im.IzvodjacSanacije
                    },
                    _ => new StetaBasic()
                };
                var bazni = (StetaBasic)dto;
                bazni.StetaId = st.StetaId; bazni.BrojStete = st.BrojStete;
                bazni.DatumPrijave = st.DatumPrijave; bazni.DatumNastanka = st.DatumNastanka;
                bazni.PolisaId = st.Polisa?.PolisaId ?? 0; bazni.BrojPolise = st.Polisa?.BrojPolise;
                bazni.PodnosilacId = st.Podnosilac?.KlijentId ?? 0; bazni.PodnosilacNaziv = st.Podnosilac?.Naziv;
                bazni.VrstaStete = st.VrstaStete; bazni.OpisDogodjaja = st.OpisDogodjaja;
                bazni.Lokacija = st.Lokacija; bazni.Status = st.Status;
                bazni.ProcenjeniIznos = st.ProcenjeniIznos; bazni.Valuta = st.Valuta;
                foreach (var f in st.FazeObrade)
                    bazni.FazeObrade.Add(new FazaObradeBasic
                    {
                        FazaId = f.FazaId, StetaId = st.StetaId,
                        RedniBrojFaze = f.RedniBrojFaze, NazivFaze = f.NazivFaze,
                        DatumPocetka = f.DatumPocetka, DatumZavrsetka = f.DatumZavrsetka,
                        OdgovornoLiceId = f.OdgovornoLice?.OsobljeId,
                        OdgovornoLiceIme = f.OdgovornoLice != null ? $"{f.OdgovornoLice.Ime} {f.OdgovornoLice.Prezime}" : null,
                        Odluka = f.Odluka, Dokumentacija = f.Dokumentacija, Napomena = f.Napomena
                    });
                foreach (var pr in st.ProceneSteta)
                    bazni.ProceneSteta.Add(new ProcenaStetaBasic
                    {
                        ProcenaId = pr.ProcenaId, StetaId = st.StetaId, DatumProc = pr.DatumProc,
                        ProceniteljId = pr.Procenitelj?.OsobljeId ?? 0,
                        ProceniteljIme = pr.Procenitelj != null ? $"{pr.Procenitelj.Ime} {pr.Procenitelj.Prezime}" : null,
                        MetodProc = pr.MetodProc, Nalaz = pr.Nalaz, ProcenjeniIznos = pr.ProcenjeniIznos, Preporuka = pr.Preporuka
                    });
                s.Close();
                return dto;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return new StetaBasic(); }
        }

        // ---------- AutoSteta ----------

        public static int dodajAutoStetu(AutoStetaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var a = new AutoSteta
                {
                    VrstaStete = "AUTO", ZapisnikPolicije = dto.ZapisnikPolicije, Servis = dto.Servis,
                    Vozilo = dto.VoziloId.HasValue ? s.Load<Vozilo>(dto.VoziloId.Value) : null
                };
                popuniBazuStete(a, dto, s);
                s.Save(a);
                s.Flush();
                s.Close();
                return a.StetaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajAutoStetu(AutoStetaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                AutoSteta a = s.Load<AutoSteta>(dto.StetaId);
                popuniBazuStete(a, dto, s);
                a.ZapisnikPolicije = dto.ZapisnikPolicije; a.Servis = dto.Servis;
                a.Vozilo = dto.VoziloId.HasValue ? s.Load<Vozilo>(dto.VoziloId.Value) : null;
                s.SaveOrUpdate(a);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- ZdravstvenaSteta ----------

        public static int dodajZdravstvenuStetu(ZdravstvenaStetaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR");
            try
            {
                ISession s = DataLayer.GetSession();
                var z = new ZdravstvenaSteta
                {
                    VrstaStete = "ZDRAVSTVENA", Dijagnoza = dto.Dijagnoza,
                    MedicinskaDocumentacija = dto.MedicinskaDocumentacija, ZdravstvenaUstanova = dto.ZdravstvenaUstanova,
                    Lekar = dto.LekarId.HasValue ? s.Load<Lekar>(dto.LekarId.Value) : null
                };
                popuniBazuStete(z, dto, s);
                s.Save(z);
                s.Flush();
                s.Close();
                return z.StetaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajZdravstvenuStetu(ZdravstvenaStetaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                ZdravstvenaSteta z = s.Load<ZdravstvenaSteta>(dto.StetaId);
                popuniBazuStete(z, dto, s);
                z.Dijagnoza = dto.Dijagnoza; z.MedicinskaDocumentacija = dto.MedicinskaDocumentacija;
                z.ZdravstvenaUstanova = dto.ZdravstvenaUstanova;
                z.Lekar = dto.LekarId.HasValue ? s.Load<Lekar>(dto.LekarId.Value) : null;
                s.SaveOrUpdate(z);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- ImovinskSteta ----------

        public static int dodajImovinskuStetu(ImovinskStetaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var im = new ImovinskSteta
                {
                    VrstaStete = "IMOVINSKA", ProcenaOstecenja = dto.ProcenaOstecenja, IzvodjacSanacije = dto.IzvodjacSanacije
                };
                popuniBazuStete(im, dto, s);
                s.Save(im);
                s.Flush();
                s.Close();
                return im.StetaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajImovinskuStetu(ImovinskStetaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                ImovinskSteta im = s.Load<ImovinskSteta>(dto.StetaId);
                popuniBazuStete(im, dto, s);
                im.ProcenaOstecenja = dto.ProcenaOstecenja; im.IzvodjacSanacije = dto.IzvodjacSanacije;
                s.SaveOrUpdate(im);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Ostecena lica (klijent registrovan ili slobodan unos imena) ----------

        public static List<OstecenLiceBasic> vratiOstecenaLicaZaStetu(int stetaId)
        {
            var lista = new List<OstecenLiceBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var o in s.Query<OstecenLice>().Where(x => x.Steta!.StetaId == stetaId))
                    lista.Add(new OstecenLiceBasic
                    {
                        OstecenLiceId = o.OstecenLiceId, StetaId = stetaId,
                        KlijentId = o.Klijent?.KlijentId, KlijentNaziv = o.Klijent?.Naziv,
                        ImePrezime = o.ImePrezime, OpisPovrede = o.OpisPovrede, IznosNaknade = o.IznosNaknade
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void dodajOstecenoLice(OstecenLiceBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                var o = new OstecenLice
                {
                    Steta = s.Load<Steta>(dto.StetaId),
                    Klijent = dto.KlijentId.HasValue ? s.Load<Klijent>(dto.KlijentId.Value) : null,
                    ImePrezime = dto.ImePrezime, OpisPovrede = dto.OpisPovrede, IznosNaknade = dto.IznosNaknade
                };
                s.Save(o);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiOstecenoLice(int id)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                OstecenLice o = s.Load<OstecenLice>(id);
                s.Delete(o);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Osteceni predmeti ----------

        public static List<OsteceniPredmetBasic> vratiOsteceniPredmetiZaStetu(int stetaId)
        {
            var lista = new List<OsteceniPredmetBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var o in s.Query<OsteceniPredmet>().Where(x => x.Steta!.StetaId == stetaId))
                    lista.Add(new OsteceniPredmetBasic
                    {
                        OsteceniPredmetId = o.OsteceniPredmetId, StetaId = stetaId,
                        TipPredmeta = o.TipPredmeta, OpisOstecenja = o.OpisOstecenja, ProcenjeniIznos = o.ProcenjeniIznos
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void dodajOsteceniPredmet(OsteceniPredmetBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                var o = new OsteceniPredmet
                {
                    Steta = s.Load<Steta>(dto.StetaId),
                    TipPredmeta = dto.TipPredmeta, OpisOstecenja = dto.OpisOstecenja, ProcenjeniIznos = dto.ProcenjeniIznos
                };
                s.Save(o);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiOsteceniPredmet(int id)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                OsteceniPredmet o = s.Load<OsteceniPredmet>(id);
                s.Delete(o);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Procene stete ----------

        public static void dodajProcenu(ProcenaStetaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                var p = new ProcenaStete
                {
                    Steta = s.Load<Steta>(dto.StetaId),
                    DatumProc = dto.DatumProc, Procenitelj = s.Load<Procenitelj>(dto.ProceniteljId),
                    MetodProc = dto.MetodProc, Nalaz = dto.Nalaz,
                    ProcenjeniIznos = dto.ProcenjeniIznos, Preporuka = dto.Preporuka
                };
                s.Save(p);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiProcenu(int id)
        {
            ProveriOvlascenje("ADMIN", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                ProcenaStete p = s.Load<ProcenaStete>(id);
                s.Delete(p);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }
    }
}
