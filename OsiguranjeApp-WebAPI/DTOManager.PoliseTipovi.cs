using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using OsiguranjApp.DTOs;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp
{
    // Kreiranje/citanje konkretnih podtipova polise (Polisa je bazna tabela,
    // ove metode rade sa "table-per-subclass" podklasama i njihovim specificnim poljima).
    public partial class DTOManager
    {
        private static Polisa popuniBazuPolise(Polisa p, PolisaPregled dto, ISession s)
        {
            p.BrojPolise = dto.BrojPolise;
            p.DatumZakljucenja = DateTime.Today;
            p.DatumPocetka = dto.DatumPocetka;
            p.DatumIsteka = dto.DatumIsteka;
            p.Status = dto.Status ?? "AKTIVNA";
            p.OsnovnaPremija = dto.OsnovnaPremija;
            p.Valuta = dto.Valuta ?? "RSD";
            p.NacinPlacanja = dto.NacinPlacanja ?? "MESECNO";
            p.Ugovarac = s.Load<Klijent>(dto.UgovaracId);
            p.Agent = dto.AgentId.HasValue ? s.Load<Agent>(dto.AgentId.Value) : null;
            return p;
        }

        // Belezi jedan red u ISTORIJA_POLISE pri svakoj izmeni postojece polise (poziva se samo iz
        // azurirajXOsiguranje metoda, ne i iz dodajXOsiguranje - istorija prati promene, ne nastanak).
        // Tip promene se izvodi iz stare/nove vrednosti statusa; ako se status nije promenio i dalje
        // se beleži generalna IZMENA, jer zadatak trazi da se prate i obicne izmene podataka polise.
        private static void ZabelezIstorijuPromene(ISession s, Polisa p, string? stariStatus)
        {
            string tip = (p.Status, stariStatus) switch
            {
                ("RASKINUTA", var stari) when stari != "RASKINUTA" => "RASKID",
                ("MIROVANJE", var stari) when stari != "MIROVANJE" => "MIROVANJE",
                ("OBNOVLJENA", var stari) when stari != "OBNOVLJENA" => "OBNOVA",
                ("AKTIVNA", "MIROVANJE") => "REAKTIVACIJA",
                ("AKTIVNA", "RASKINUTA") => "REAKTIVACIJA",
                _ => "IZMENA"
            };
            string opis = stariStatus != p.Status
                ? $"Status promenjen: {stariStatus} -> {p.Status}"
                : "Izmena podataka polise";
            int? osobljeId = SesijaKorisnik.TrenutniNalog?.OsobljeId;

            s.Save(new IstorijaPolise
            {
                Polisa = p,
                TipPromene = tip,
                DatumPromene = DateTime.Now,
                Opis = opis,
                Korisnik = osobljeId.HasValue ? s.Load<Osoblje>(osobljeId.Value) : null
            });
        }

        // ---------- AutoOsiguranje ----------

        public static List<AutoPolisaPregled> vratiSvaAutoOsiguranja()
        {
            var lista = new List<AutoPolisaPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var a in s.Query<AutoOsiguranje>().OrderByDescending(x => x.DatumZakljucenja))
                    lista.Add(mapAutoPolisaPregled(a));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajAutoOsiguranje(AutoPolisaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var a = new AutoOsiguranje
                {
                    TipOsiguranja = "AUTO",
                    Vozilo = s.Load<Vozilo>(dto.VoziloId),
                    BonusMalusKlasa = dto.BonusMalusKlasa,
                    TeritorijanoVazenje = dto.TeritorijanoVazenje
                };
                popuniBazuPolise(a, dto, s);
                foreach (var klijentId in dto.VoziciIds)
                    a.Vozaci.Add(s.Load<Klijent>(klijentId));
                s.Save(a);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajAutoOsiguranje(AutoPolisaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                AutoOsiguranje a = s.Load<AutoOsiguranje>(dto.PolisaId);
                string? stariStatus = a.Status;
                popuniBazuPolise(a, dto, s);
                a.Vozilo = s.Load<Vozilo>(dto.VoziloId);
                a.BonusMalusKlasa = dto.BonusMalusKlasa;
                a.TeritorijanoVazenje = dto.TeritorijanoVazenje;
                a.Vozaci.Clear();
                foreach (var klijentId in dto.VoziciIds)
                    a.Vozaci.Add(s.Load<Klijent>(klijentId));
                ZabelezIstorijuPromene(s, a, stariStatus);
                s.SaveOrUpdate(a);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static AutoPolisaPregled mapAutoPolisaPregled(AutoOsiguranje a) => new AutoPolisaPregled
        {
            PolisaId = a.PolisaId, BrojPolise = a.BrojPolise, TipOsiguranja = a.TipOsiguranja,
            DatumPocetka = a.DatumPocetka, DatumIsteka = a.DatumIsteka, Status = a.Status,
            OsnovnaPremija = a.OsnovnaPremija, Valuta = a.Valuta, NacinPlacanja = a.NacinPlacanja,
            UgovaracId = a.Ugovarac?.KlijentId ?? 0, UgovaracNaziv = a.Ugovarac?.Naziv,
            AgentId = a.Agent?.OsobljeId, AgentIme = a.Agent != null ? $"{a.Agent.Ime} {a.Agent.Prezime}" : null,
            VoziloId = a.Vozilo?.VoziloId ?? 0, VoziloOpis = a.Vozilo?.ToString(),
            BonusMalusKlasa = a.BonusMalusKlasa, TeritorijanoVazenje = a.TeritorijanoVazenje,
            VoziciIds = a.Vozaci.Select(v => v.KlijentId).ToList()
        };

        // ---------- ZivotnoOsiguranje ----------

        public static List<ZivotnoPregled> vratiSvaZivotnaOsiguranja()
        {
            var lista = new List<ZivotnoPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var z in s.Query<ZivotnoOsiguranje>().OrderByDescending(x => x.DatumZakljucenja))
                    lista.Add(mapZivotnoPregled(z));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajZivotnoOsiguranje(ZivotnoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var z = new ZivotnoOsiguranje
                {
                    TipOsiguranja = "ZIVOTNO",
                    SumaOsiguranja = dto.SumaOsiguranja,
                    TipIsplate = dto.TipIsplate
                };
                popuniBazuPolise(z, dto, s);
                s.Save(z);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajZivotnoOsiguranje(ZivotnoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                ZivotnoOsiguranje z = s.Load<ZivotnoOsiguranje>(dto.PolisaId);
                string? stariStatus = z.Status;
                popuniBazuPolise(z, dto, s);
                z.SumaOsiguranja = dto.SumaOsiguranja;
                z.TipIsplate = dto.TipIsplate;
                ZabelezIstorijuPromene(s, z, stariStatus);
                s.SaveOrUpdate(z);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static ZivotnoPregled mapZivotnoPregled(ZivotnoOsiguranje z) => new ZivotnoPregled
        {
            PolisaId = z.PolisaId, BrojPolise = z.BrojPolise, TipOsiguranja = z.TipOsiguranja,
            DatumPocetka = z.DatumPocetka, DatumIsteka = z.DatumIsteka, Status = z.Status,
            OsnovnaPremija = z.OsnovnaPremija, Valuta = z.Valuta, NacinPlacanja = z.NacinPlacanja,
            UgovaracId = z.Ugovarac?.KlijentId ?? 0, UgovaracNaziv = z.Ugovarac?.Naziv,
            AgentId = z.Agent?.OsobljeId, AgentIme = z.Agent != null ? $"{z.Agent.Ime} {z.Agent.Prezime}" : null,
            SumaOsiguranja = z.SumaOsiguranja, TipIsplate = z.TipIsplate
        };

        // ---------- ZdravstvenoOsiguranje ----------

        public static List<ZdravstvenoPregled> vratiSvaZdravstvenaOsiguranja()
        {
            var lista = new List<ZdravstvenoPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var z in s.Query<ZdravstvenoOsiguranje>().OrderByDescending(x => x.DatumZakljucenja))
                    lista.Add(mapZdravstvenoPregled(z));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajZdravstvenoOsiguranje(ZdravstvenoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var z = new ZdravstvenoOsiguranje
                {
                    TipOsiguranja = "ZDRAVSTVENO",
                    MrezaUstanova = dto.MrezaUstanova,
                    LimitSpecijalista = dto.LimitSpecijalista,
                    LimitStomatologa = dto.LimitStomatologa,
                    LimitBolnickih = dto.LimitBolnickih,
                    LimitBolnickiDan = dto.LimitBolnickiDan,
                    Pokrica = dto.Pokrica
                };
                popuniBazuPolise(z, dto, s);
                s.Save(z);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajZdravstvenoOsiguranje(ZdravstvenoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                ZdravstvenoOsiguranje z = s.Load<ZdravstvenoOsiguranje>(dto.PolisaId);
                string? stariStatus = z.Status;
                popuniBazuPolise(z, dto, s);
                z.MrezaUstanova = dto.MrezaUstanova;
                z.LimitSpecijalista = dto.LimitSpecijalista;
                z.LimitStomatologa = dto.LimitStomatologa;
                z.LimitBolnickih = dto.LimitBolnickih;
                z.LimitBolnickiDan = dto.LimitBolnickiDan;
                z.Pokrica = dto.Pokrica;
                ZabelezIstorijuPromene(s, z, stariStatus);
                s.SaveOrUpdate(z);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static ZdravstvenoPregled mapZdravstvenoPregled(ZdravstvenoOsiguranje z) => new ZdravstvenoPregled
        {
            PolisaId = z.PolisaId, BrojPolise = z.BrojPolise, TipOsiguranja = z.TipOsiguranja,
            DatumPocetka = z.DatumPocetka, DatumIsteka = z.DatumIsteka, Status = z.Status,
            OsnovnaPremija = z.OsnovnaPremija, Valuta = z.Valuta, NacinPlacanja = z.NacinPlacanja,
            UgovaracId = z.Ugovarac?.KlijentId ?? 0, UgovaracNaziv = z.Ugovarac?.Naziv,
            AgentId = z.Agent?.OsobljeId, AgentIme = z.Agent != null ? $"{z.Agent.Ime} {z.Agent.Prezime}" : null,
            MrezaUstanova = z.MrezaUstanova, LimitSpecijalista = z.LimitSpecijalista,
            LimitStomatologa = z.LimitStomatologa, LimitBolnickih = z.LimitBolnickih,
            LimitBolnickiDan = z.LimitBolnickiDan, Pokrica = z.Pokrica
        };

        // ---------- PutnoOsiguranje ----------

        public static List<PutnoPregled> vratiSvaPutnaOsiguranja()
        {
            var lista = new List<PutnoPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var p in s.Query<PutnoOsiguranje>().OrderByDescending(x => x.DatumZakljucenja))
                    lista.Add(mapPutnoPregled(p));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajPutnoOsiguranje(PutnoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var p = new PutnoOsiguranje
                {
                    TipOsiguranja = "PUTNO",
                    Destinacije = dto.Destinacije,
                    DatumPolaska = dto.DatumPolaska,
                    DatumPovratka = dto.DatumPovratka
                };
                popuniBazuPolise(p, dto, s);
                foreach (var klijentId in dto.OsiguranaLicaIds)
                    p.OsiguranaLica.Add(s.Load<Klijent>(klijentId));
                s.Save(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajPutnoOsiguranje(PutnoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                PutnoOsiguranje p = s.Load<PutnoOsiguranje>(dto.PolisaId);
                string? stariStatus = p.Status;
                popuniBazuPolise(p, dto, s);
                p.Destinacije = dto.Destinacije;
                p.DatumPolaska = dto.DatumPolaska;
                p.DatumPovratka = dto.DatumPovratka;
                p.OsiguranaLica.Clear();
                foreach (var klijentId in dto.OsiguranaLicaIds)
                    p.OsiguranaLica.Add(s.Load<Klijent>(klijentId));
                ZabelezIstorijuPromene(s, p, stariStatus);
                s.SaveOrUpdate(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static PutnoPregled mapPutnoPregled(PutnoOsiguranje p) => new PutnoPregled
        {
            PolisaId = p.PolisaId, BrojPolise = p.BrojPolise, TipOsiguranja = p.TipOsiguranja,
            DatumPocetka = p.DatumPocetka, DatumIsteka = p.DatumIsteka, Status = p.Status,
            OsnovnaPremija = p.OsnovnaPremija, Valuta = p.Valuta, NacinPlacanja = p.NacinPlacanja,
            UgovaracId = p.Ugovarac?.KlijentId ?? 0, UgovaracNaziv = p.Ugovarac?.Naziv,
            AgentId = p.Agent?.OsobljeId, AgentIme = p.Agent != null ? $"{p.Agent.Ime} {p.Agent.Prezime}" : null,
            Destinacije = p.Destinacije, DatumPolaska = p.DatumPolaska, DatumPovratka = p.DatumPovratka,
            OsiguranaLicaIds = p.OsiguranaLica.Select(k => k.KlijentId).ToList()
        };

        // ---------- ImovinskOsiguranje ----------

        public static List<ImovinskoPregled> vratiSvaImovinskaOsiguranja()
        {
            var lista = new List<ImovinskoPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var i in s.Query<ImovinskOsiguranje>().OrderByDescending(x => x.DatumZakljucenja))
                    lista.Add(mapImovinskoPregled(i));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajImovinskoOsiguranje(ImovinskoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var i = new ImovinskOsiguranje
                {
                    TipOsiguranja = "IMOVINSKO",
                    VrsteRizika = dto.VrsteRizika
                };
                popuniBazuPolise(i, dto, s);
                foreach (var nekretninaId in dto.NekretnineIds)
                    i.Nekretnine.Add(s.Load<Nekretnina>(nekretninaId));
                foreach (var pokretnaId in dto.PokretneImovineIds)
                    i.PokretneImovine.Add(s.Load<PokretnaImovina>(pokretnaId));
                s.Save(i);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajImovinskoOsiguranje(ImovinskoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                ImovinskOsiguranje i = s.Load<ImovinskOsiguranje>(dto.PolisaId);
                string? stariStatus = i.Status;
                popuniBazuPolise(i, dto, s);
                i.VrsteRizika = dto.VrsteRizika;
                i.Nekretnine.Clear();
                foreach (var nekretninaId in dto.NekretnineIds)
                    i.Nekretnine.Add(s.Load<Nekretnina>(nekretninaId));
                i.PokretneImovine.Clear();
                foreach (var pokretnaId in dto.PokretneImovineIds)
                    i.PokretneImovine.Add(s.Load<PokretnaImovina>(pokretnaId));
                ZabelezIstorijuPromene(s, i, stariStatus);
                s.SaveOrUpdate(i);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static ImovinskoPregled mapImovinskoPregled(ImovinskOsiguranje i) => new ImovinskoPregled
        {
            PolisaId = i.PolisaId, BrojPolise = i.BrojPolise, TipOsiguranja = i.TipOsiguranja,
            DatumPocetka = i.DatumPocetka, DatumIsteka = i.DatumIsteka, Status = i.Status,
            OsnovnaPremija = i.OsnovnaPremija, Valuta = i.Valuta, NacinPlacanja = i.NacinPlacanja,
            UgovaracId = i.Ugovarac?.KlijentId ?? 0, UgovaracNaziv = i.Ugovarac?.Naziv,
            AgentId = i.Agent?.OsobljeId, AgentIme = i.Agent != null ? $"{i.Agent.Ime} {i.Agent.Prezime}" : null,
            VrsteRizika = i.VrsteRizika,
            NekretnineIds = i.Nekretnine.Select(n => n.NekretninaId).ToList(),
            PokretneImovineIds = i.PokretneImovine.Select(pi => pi.PokretnaImovinaId).ToList()
        };

        // ---------- PoljoprivrednoOsiguranje ----------

        public static List<PoljoprivrednoPregled> vratiSvaPoljoprivrednaOsiguranja()
        {
            var lista = new List<PoljoprivrednoPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var p in s.Query<PoljoprivrednoOsiguranje>().OrderByDescending(x => x.DatumZakljucenja))
                    lista.Add(mapPoljoprivrednoPregled(p));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajPoljoprivrednoOsiguranje(PoljoprivrednoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var p = new PoljoprivrednoOsiguranje { TipOsiguranja = "POLJOPRIVREDNO" };
                popuniBazuPolise(p, dto, s);
                foreach (var usevId in dto.UseviIds)
                    p.Usevi.Add(s.Load<Usev>(usevId));
                foreach (var zivotinjaId in dto.ZivotinjeIds)
                    p.Zivotinje.Add(s.Load<Zivotinja>(zivotinjaId));
                s.Save(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajPoljoprivrednoOsiguranje(PoljoprivrednoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                PoljoprivrednoOsiguranje p = s.Load<PoljoprivrednoOsiguranje>(dto.PolisaId);
                string? stariStatus = p.Status;
                popuniBazuPolise(p, dto, s);
                p.Usevi.Clear();
                foreach (var usevId in dto.UseviIds)
                    p.Usevi.Add(s.Load<Usev>(usevId));
                p.Zivotinje.Clear();
                foreach (var zivotinjaId in dto.ZivotinjeIds)
                    p.Zivotinje.Add(s.Load<Zivotinja>(zivotinjaId));
                ZabelezIstorijuPromene(s, p, stariStatus);
                s.SaveOrUpdate(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static PoljoprivrednoPregled mapPoljoprivrednoPregled(PoljoprivrednoOsiguranje p) => new PoljoprivrednoPregled
        {
            PolisaId = p.PolisaId, BrojPolise = p.BrojPolise, TipOsiguranja = p.TipOsiguranja,
            DatumPocetka = p.DatumPocetka, DatumIsteka = p.DatumIsteka, Status = p.Status,
            OsnovnaPremija = p.OsnovnaPremija, Valuta = p.Valuta, NacinPlacanja = p.NacinPlacanja,
            UgovaracId = p.Ugovarac?.KlijentId ?? 0, UgovaracNaziv = p.Ugovarac?.Naziv,
            AgentId = p.Agent?.OsobljeId, AgentIme = p.Agent != null ? $"{p.Agent.Ime} {p.Agent.Prezime}" : null,
            UseviIds = p.Usevi.Select(u => u.UsevId).ToList(),
            ZivotinjeIds = p.Zivotinje.Select(z => z.ZivotinjaId).ToList()
        };

        // ---------- OsiguranjeOdgovornosti ----------

        public static List<OdgovornostPregled> vratiSvaOsiguranjaOdgovornosti()
        {
            var lista = new List<OdgovornostPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var o in s.Query<OsiguranjeOdgovornosti>().OrderByDescending(x => x.DatumZakljucenja))
                    lista.Add(mapOdgovornostPregled(o));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajOsiguranjeOdgovornosti(OdgovornostPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var o = new OsiguranjeOdgovornosti
                {
                    TipOsiguranja = "ODGOVORNOST",
                    VrstaOdgovornosti = dto.VrstaOdgovornosti,
                    LimitOdgovornosti = dto.LimitOdgovornosti
                };
                popuniBazuPolise(o, dto, s);
                s.Save(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajOsiguranjeOdgovornosti(OdgovornostPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                OsiguranjeOdgovornosti o = s.Load<OsiguranjeOdgovornosti>(dto.PolisaId);
                string? stariStatus = o.Status;
                popuniBazuPolise(o, dto, s);
                o.VrstaOdgovornosti = dto.VrstaOdgovornosti;
                o.LimitOdgovornosti = dto.LimitOdgovornosti;
                ZabelezIstorijuPromene(s, o, stariStatus);
                s.SaveOrUpdate(o);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static OdgovornostPregled mapOdgovornostPregled(OsiguranjeOdgovornosti o) => new OdgovornostPregled
        {
            PolisaId = o.PolisaId, BrojPolise = o.BrojPolise, TipOsiguranja = o.TipOsiguranja,
            DatumPocetka = o.DatumPocetka, DatumIsteka = o.DatumIsteka, Status = o.Status,
            OsnovnaPremija = o.OsnovnaPremija, Valuta = o.Valuta, NacinPlacanja = o.NacinPlacanja,
            UgovaracId = o.Ugovarac?.KlijentId ?? 0, UgovaracNaziv = o.Ugovarac?.Naziv,
            AgentId = o.Agent?.OsobljeId, AgentIme = o.Agent != null ? $"{o.Agent.Ime} {o.Agent.Prezime}" : null,
            VrstaOdgovornosti = o.VrstaOdgovornosti, LimitOdgovornosti = o.LimitOdgovornosti
        };

        // ---------- SpecijalizovanoOsiguranje ----------

        public static List<SpecijalizovanoPregled> vratiSvaSpecijalizovanaOsiguranja()
        {
            var lista = new List<SpecijalizovanoPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var sp in s.Query<SpecijalizovanoOsiguranje>().OrderByDescending(x => x.DatumZakljucenja))
                    lista.Add(mapSpecijalizovanoPregled(sp));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajSpecijalizovanoOsiguranje(SpecijalizovanoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var sp = new SpecijalizovanoOsiguranje
                {
                    TipOsiguranja = "SPECIJALIZOVANO",
                    NazivSpecijalizacije = dto.NazivSpecijalizacije,
                    OpisUslova = dto.OpisUslova
                };
                popuniBazuPolise(sp, dto, s);
                s.Save(sp);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajSpecijalizovanoOsiguranje(SpecijalizovanoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                SpecijalizovanoOsiguranje sp = s.Load<SpecijalizovanoOsiguranje>(dto.PolisaId);
                string? stariStatus = sp.Status;
                popuniBazuPolise(sp, dto, s);
                sp.NazivSpecijalizacije = dto.NazivSpecijalizacije;
                sp.OpisUslova = dto.OpisUslova;
                ZabelezIstorijuPromene(s, sp, stariStatus);
                s.SaveOrUpdate(sp);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static SpecijalizovanoPregled mapSpecijalizovanoPregled(SpecijalizovanoOsiguranje sp) => new SpecijalizovanoPregled
        {
            PolisaId = sp.PolisaId, BrojPolise = sp.BrojPolise, TipOsiguranja = sp.TipOsiguranja,
            DatumPocetka = sp.DatumPocetka, DatumIsteka = sp.DatumIsteka, Status = sp.Status,
            OsnovnaPremija = sp.OsnovnaPremija, Valuta = sp.Valuta, NacinPlacanja = sp.NacinPlacanja,
            UgovaracId = sp.Ugovarac?.KlijentId ?? 0, UgovaracNaziv = sp.Ugovarac?.Naziv,
            AgentId = sp.Agent?.OsobljeId, AgentIme = sp.Agent != null ? $"{sp.Agent.Ime} {sp.Agent.Prezime}" : null,
            NazivSpecijalizacije = sp.NazivSpecijalizacije, OpisUslova = sp.OpisUslova
        };
    }
}
