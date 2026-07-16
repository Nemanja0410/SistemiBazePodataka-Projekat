using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using OsiguranjApp.DTOs;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp
{
    // CRUD za "detaljne" (child) entitete koji vise pripadaju nekom drugom entitetu
    // (polisi, steti, klijentu...) nego sto su samostalni agregatni koreni.
    public partial class DTOManager
    {
        // ---------- DodatnoPokrice ----------

        public static List<DodatnoPokrBasic> vratiDodatnaPokricaZaPolisu(int polisaId)
        {
            var lista = new List<DodatnoPokrBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var dp in s.Query<DodatnoPokrice>().Where(x => x.Polisa!.PolisaId == polisaId))
                    lista.Add(mapDodatnoPokrBasic(dp));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajDodatnoPokrice(DodatnoPokrBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var dp = new DodatnoPokrice
                {
                    Polisa = s.Load<Polisa>(dto.PolisaId),
                    Naziv = dto.Naziv, Opis = dto.Opis,
                    LimitPokrića = dto.LimitPokrića, Fransiza = dto.Fransiza,
                    DodatnaPremija = dto.DodatnaPremija
                };
                s.Save(dp);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajDodatnoPokrice(DodatnoPokrBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                DodatnoPokrice dp = s.Load<DodatnoPokrice>(dto.PokriceId);
                dp.Naziv = dto.Naziv; dp.Opis = dto.Opis;
                dp.LimitPokrića = dto.LimitPokrića; dp.Fransiza = dto.Fransiza;
                dp.DodatnaPremija = dto.DodatnaPremija;
                s.SaveOrUpdate(dp);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiDodatnoPokrice(int id)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                DodatnoPokrice dp = s.Load<DodatnoPokrice>(id);
                s.Delete(dp);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static DodatnoPokrBasic mapDodatnoPokrBasic(DodatnoPokrice dp) => new DodatnoPokrBasic
        {
            PokriceId = dp.PokriceId, PolisaId = dp.Polisa?.PolisaId ?? 0,
            Naziv = dp.Naziv, Opis = dp.Opis,
            LimitPokrića = dp.LimitPokrića, Fransiza = dp.Fransiza,
            DodatnaPremija = dp.DodatnaPremija
        };

        // ---------- IstorijaPolise (log - samo dodavanje i citanje) ----------

        public static List<IstorijaPoliseBasic> vratiIstorijuPolise(int polisaId)
        {
            var lista = new List<IstorijaPoliseBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var ip in s.Query<IstorijaPolise>()
                             .Where(x => x.Polisa!.PolisaId == polisaId)
                             .OrderByDescending(x => x.DatumPromene))
                    lista.Add(new IstorijaPoliseBasic
                    {
                        IstorijaId = ip.IstorijaId, PolisaId = polisaId,
                        TipPromene = ip.TipPromene, DatumPromene = ip.DatumPromene,
                        Opis = ip.Opis, KorisnikId = ip.Korisnik?.OsobljeId,
                        KorisnikIme = ip.Korisnik != null ? $"{ip.Korisnik.Ime} {ip.Korisnik.Prezime}" : null
                    });
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajIstorijuPolise(IstorijaPoliseBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                int? korisnikId = dto.KorisnikId ?? SesijaKorisnik.TrenutniNalog?.OsobljeId;
                var ip = new IstorijaPolise
                {
                    Polisa = s.Load<Polisa>(dto.PolisaId),
                    TipPromene = dto.TipPromene,
                    DatumPromene = DateTime.Now,
                    Opis = dto.Opis,
                    Korisnik = korisnikId.HasValue ? s.Load<Osoblje>(korisnikId.Value) : null
                };
                s.Save(ip);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        // ---------- UlogaKlijenta ----------

        public static List<UlogaKlijentaBasic> vratiUlogeZaPolisu(int polisaId)
        {
            var lista = new List<UlogaKlijentaBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var u in s.Query<UlogaKlijenta>().Where(x => x.Polisa!.PolisaId == polisaId))
                    lista.Add(mapUlogaKlijentaBasic(u));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajUlogu(UlogaKlijentaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var u = new UlogaKlijenta
                {
                    Klijent = dto.KlijentId.HasValue ? s.Load<Klijent>(dto.KlijentId.Value) : null,
                    ImePrezime = dto.ImePrezime,
                    Polisa = s.Load<Polisa>(dto.PolisaId),
                    TipUloge = dto.TipUloge
                };
                s.Save(u);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajUlogu(UlogaKlijentaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                UlogaKlijenta u = s.Load<UlogaKlijenta>(dto.UlogaId);
                u.TipUloge = dto.TipUloge;
                s.SaveOrUpdate(u);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiUlogu(int id)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                UlogaKlijenta u = s.Load<UlogaKlijenta>(id);
                s.Delete(u);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static UlogaKlijentaBasic mapUlogaKlijentaBasic(UlogaKlijenta u) => new UlogaKlijentaBasic
        {
            UlogaId = u.UlogaId,
            KlijentId = u.Klijent?.KlijentId, KlijentNaziv = u.Klijent?.Naziv,
            ImePrezime = u.ImePrezime,
            PolisaId = u.Polisa?.PolisaId ?? 0, BrojPolise = u.Polisa?.BrojPolise,
            TipUloge = u.TipUloge
        };

        // ---------- KontaktOsoba ----------

        public static List<KontaktOsobaBasic> vratiKontakteZaKlijenta(int klijentId)
        {
            var lista = new List<KontaktOsobaBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var k in s.Query<KontaktOsoba>().Where(x => x.Klijent!.KlijentId == klijentId))
                    lista.Add(mapKontaktOsobaBasic(k));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajKontaktOsobu(KontaktOsobaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var k = new KontaktOsoba
                {
                    Klijent = s.Load<Klijent>(dto.KlijentId),
                    Ime = dto.Ime, Prezime = dto.Prezime,
                    Telefon = dto.Telefon, Email = dto.Email, Funkcija = dto.Funkcija
                };
                s.Save(k);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajKontaktOsobu(KontaktOsobaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                KontaktOsoba k = s.Load<KontaktOsoba>(dto.KontaktId);
                k.Ime = dto.Ime; k.Prezime = dto.Prezime;
                k.Telefon = dto.Telefon; k.Email = dto.Email; k.Funkcija = dto.Funkcija;
                s.SaveOrUpdate(k);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiKontaktOsobu(int id)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                KontaktOsoba k = s.Load<KontaktOsoba>(id);
                s.Delete(k);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static KontaktOsobaBasic mapKontaktOsobaBasic(KontaktOsoba k) => new KontaktOsobaBasic
        {
            KontaktId = k.KontaktId, KlijentId = k.Klijent?.KlijentId ?? 0,
            Ime = k.Ime, Prezime = k.Prezime, Telefon = k.Telefon,
            Email = k.Email, Funkcija = k.Funkcija
        };

        // ---------- OblastProc ----------

        public static List<OblastProcBasic> vratiOblastiZaProcenitelja(int proceniteljId)
        {
            var lista = new List<OblastProcBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var o in s.Query<OblastProc>().Where(x => x.Procenitelj!.OsobljeId == proceniteljId))
                    lista.Add(new OblastProcBasic
                    {
                        OblastId = o.OblastId, ProceniteljId = o.Procenitelj?.OsobljeId ?? 0,
                        ProceniteljIme = o.Procenitelj != null ? $"{o.Procenitelj.Ime} {o.Procenitelj.Prezime}" : null,
                        Oblast = o.Oblast
                    });
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajOblastProc(OblastProcBasic dto)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                var o = new OblastProc
                {
                    Procenitelj = s.Load<Procenitelj>(dto.ProceniteljId),
                    Oblast = dto.Oblast
                };
                s.Save(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajOblastProc(OblastProcBasic dto)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                OblastProc o = s.Load<OblastProc>(dto.OblastId);
                o.Oblast = dto.Oblast;
                s.SaveOrUpdate(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiOblastProc(int id)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                OblastProc o = s.Load<OblastProc>(id);
                s.Delete(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        // ---------- OsteceniPredmet ----------

        public static List<OsteceniPredmetBasic> vratiOsteceniPredmetiZaStetu(int stetaId)
        {
            var lista = new List<OsteceniPredmetBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var o in s.Query<OsteceniPredmet>().Where(x => x.Steta!.StetaId == stetaId))
                    lista.Add(mapOsteceniPredmetBasic(o));
                s.Close();
            }
            catch (Exception) { throw; }
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
                    TipPredmeta = dto.TipPredmeta, OpisOstecenja = dto.OpisOstecenja,
                    ProcenjeniIznos = dto.ProcenjeniIznos
                };
                s.Save(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajOsteceniPredmet(OsteceniPredmetBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                OsteceniPredmet o = s.Load<OsteceniPredmet>(dto.OsteceniPredmetId);
                o.TipPredmeta = dto.TipPredmeta; o.OpisOstecenja = dto.OpisOstecenja;
                o.ProcenjeniIznos = dto.ProcenjeniIznos;
                s.SaveOrUpdate(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiOsteceniPredmet(int id)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                OsteceniPredmet o = s.Load<OsteceniPredmet>(id);
                s.Delete(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static OsteceniPredmetBasic mapOsteceniPredmetBasic(OsteceniPredmet o) => new OsteceniPredmetBasic
        {
            OsteceniPredmetId = o.OsteceniPredmetId, StetaId = o.Steta?.StetaId ?? 0,
            TipPredmeta = o.TipPredmeta, OpisOstecenja = o.OpisOstecenja,
            ProcenjeniIznos = o.ProcenjeniIznos
        };

        // ---------- OstecenLice ----------

        public static List<OstecenLiceBasic> vratiOstecenaLicaZaStetu(int stetaId)
        {
            var lista = new List<OstecenLiceBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var o in s.Query<OstecenLice>().Where(x => x.Steta!.StetaId == stetaId))
                    lista.Add(mapOstecenLiceBasic(o));
                s.Close();
            }
            catch (Exception) { throw; }
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
                    ImePrezime = dto.ImePrezime, OpisPovrede = dto.OpisPovrede,
                    IznosNaknade = dto.IznosNaknade
                };
                s.Save(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajOstecenoLice(OstecenLiceBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                OstecenLice o = s.Load<OstecenLice>(dto.OstecenLiceId);
                o.ImePrezime = dto.ImePrezime; o.OpisPovrede = dto.OpisPovrede;
                o.IznosNaknade = dto.IznosNaknade;
                s.SaveOrUpdate(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiOstecenoLice(int id)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                OstecenLice o = s.Load<OstecenLice>(id);
                s.Delete(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static OstecenLiceBasic mapOstecenLiceBasic(OstecenLice o) => new OstecenLiceBasic
        {
            OstecenLiceId = o.OstecenLiceId, StetaId = o.Steta?.StetaId ?? 0,
            KlijentId = o.Klijent?.KlijentId, KlijentNaziv = o.Klijent?.Naziv,
            ImePrezime = o.ImePrezime, OpisPovrede = o.OpisPovrede,
            IznosNaknade = o.IznosNaknade
        };

        // ---------- ProcenaStete ----------

        public static List<ProcenaStetaBasic> vratiProceneZaStetu(int stetaId)
        {
            var lista = new List<ProcenaStetaBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var p in s.Query<ProcenaStete>().Where(x => x.Steta!.StetaId == stetaId))
                    lista.Add(mapProcenaStetaBasic(p));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajProcenu(ProcenaStetaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                var p = new ProcenaStete
                {
                    Steta = s.Load<Steta>(dto.StetaId),
                    DatumProc = DateTime.Now,
                    Procenitelj = s.Load<Procenitelj>(dto.ProceniteljId),
                    MetodProc = dto.MetodProc, Nalaz = dto.Nalaz,
                    ProcenjeniIznos = dto.ProcenjeniIznos, Preporuka = dto.Preporuka
                };
                s.Save(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajProcenu(ProcenaStetaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                ProcenaStete p = s.Load<ProcenaStete>(dto.ProcenaId);
                p.MetodProc = dto.MetodProc; p.Nalaz = dto.Nalaz;
                p.ProcenjeniIznos = dto.ProcenjeniIznos; p.Preporuka = dto.Preporuka;
                s.SaveOrUpdate(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiProcenu(int id)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                ProcenaStete p = s.Load<ProcenaStete>(id);
                s.Delete(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static ProcenaStetaBasic mapProcenaStetaBasic(ProcenaStete p)
        {
            var dto = new ProcenaStetaBasic
            {
                ProcenaId = p.ProcenaId, StetaId = p.Steta?.StetaId ?? 0,
                DatumProc = p.DatumProc,
                ProceniteljId = p.Procenitelj?.OsobljeId ?? 0,
                ProceniteljIme = p.Procenitelj != null ? $"{p.Procenitelj.Ime} {p.Procenitelj.Prezime}" : null,
                ProceniteljBrojLicence = p.Procenitelj?.BrojLicence,
                MetodProc = p.MetodProc, Nalaz = p.Nalaz,
                ProcenjeniIznos = p.ProcenjeniIznos, Preporuka = p.Preporuka
            };
            if (p.Procenitelj != null)
                foreach (var ob in p.Procenitelj.OblasiProc)
                    if (ob.Oblast != null) dto.ProceniteljOblasti.Add(ob.Oblast);
            return dto;
        }

        // ---------- KorisnikIsplate ----------

        public static List<KorisnikIsplateBasic> vratiKorisnikeIsplateZaPolisu(int polisaId)
        {
            var lista = new List<KorisnikIsplateBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var k in s.Query<KorisnikIsplate>().Where(x => x.Polisa!.PolisaId == polisaId))
                    lista.Add(new KorisnikIsplateBasic
                    {
                        KorisnikId = k.KorisnikId, PolisaId = k.Polisa?.PolisaId ?? 0,
                        KlijentId = k.Klijent?.KlijentId, KlijentNaziv = k.Klijent?.Naziv,
                        ImePrezime = k.ImePrezime,
                        ProcenatUdela = k.ProcenatUdela
                    });
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajKorisnikaIsplate(KorisnikIsplateBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var k = new KorisnikIsplate
                {
                    Polisa = s.Load<ZivotnoOsiguranje>(dto.PolisaId),
                    Klijent = dto.KlijentId.HasValue ? s.Load<Klijent>(dto.KlijentId.Value) : null,
                    ImePrezime = dto.ImePrezime,
                    ProcenatUdela = dto.ProcenatUdela
                };
                s.Save(k);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajKorisnikaIsplate(KorisnikIsplateBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                KorisnikIsplate k = s.Load<KorisnikIsplate>(dto.KorisnikId);
                k.ProcenatUdela = dto.ProcenatUdela;
                s.SaveOrUpdate(k);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiKorisnikaIsplate(int id)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                KorisnikIsplate k = s.Load<KorisnikIsplate>(id);
                s.Delete(k);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        // ---------- Nekretnina ----------

        public static List<NekretninaBasic> vratiSveNekretnine()
        {
            var lista = new List<NekretninaBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var n in s.Query<Nekretnina>().OrderBy(x => x.Adresa))
                    lista.Add(mapNekretninaBasic(n));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajNekretninu(NekretninaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var n = new Nekretnina
                {
                    Adresa = dto.Adresa, TipObjekta = dto.TipObjekta,
                    Povrsina = dto.Povrsina, GodinaIzgradnje = dto.GodinaIzgradnje,
                    ProcenjenaVrednost = dto.ProcenjenaVrednost
                };
                s.Save(n);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajNekretninu(NekretninaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                Nekretnina n = s.Load<Nekretnina>(dto.NekretninaId);
                n.Adresa = dto.Adresa; n.TipObjekta = dto.TipObjekta;
                n.Povrsina = dto.Povrsina; n.GodinaIzgradnje = dto.GodinaIzgradnje;
                n.ProcenjenaVrednost = dto.ProcenjenaVrednost;
                s.SaveOrUpdate(n);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiNekretninu(int id)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Nekretnina n = s.Load<Nekretnina>(id);
                s.Delete(n);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static NekretninaBasic mapNekretninaBasic(Nekretnina n) => new NekretninaBasic
        {
            NekretninaId = n.NekretninaId, Adresa = n.Adresa, TipObjekta = n.TipObjekta,
            Povrsina = n.Povrsina, GodinaIzgradnje = n.GodinaIzgradnje,
            ProcenjenaVrednost = n.ProcenjenaVrednost
        };

        // ---------- Vozilo: dopuna (Read + Create vec postoje u DTOManager.Stete.cs) ----------

        public static void azurirajVozilo(VoziloBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                Vozilo v = s.Load<Vozilo>(dto.VoziloId);
                v.Registracija = dto.Registracija; v.Marka = dto.Marka; v.Model = dto.Model;
                v.GodinaProizvodnje = dto.GodinaProizvodnje;
                v.Vlasnik = s.Load<Klijent>(dto.VlasnikId);
                s.SaveOrUpdate(v);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiVozilo(int id)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Vozilo v = s.Load<Vozilo>(id);
                s.Delete(v);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }
    }
}
