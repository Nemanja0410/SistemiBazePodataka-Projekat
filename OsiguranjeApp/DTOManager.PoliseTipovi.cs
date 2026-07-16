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
    // Tip-specificna polja polise (Zivotno/Zdravstveno/Auto/Imovinsko/Putno) + osiguranici,
    // dodatna pokrica i korisnici isplate koji se upisuju direktno kroz Dodaj/Izmeni polisu.
    public partial class DTOManager
    {
        // Snimak zajednickih polja polise pre izmene - hvata se odmah posle s.Load, pre nego
        // sto popuniBazuPolise prepise vrednosti, da bi ZabelezIstorijuPromene mogla da napravi
        // detaljan opis "staro -> novo" umesto generickog "Izmena podataka polise".
        private readonly struct StaroStanjePolise
        {
            public string? Status { get; init; }
            public decimal OsnovnaPremija { get; init; }
            public string? Valuta { get; init; }
            public string? NacinPlacanja { get; init; }
            public int? AgentId { get; init; }
            public string? AgentIme { get; init; }
            public DateTime DatumIsteka { get; init; }
        }

        private static StaroStanjePolise UhvatiStaroStanjePolise(Polisa p) => new()
        {
            Status = p.Status, OsnovnaPremija = p.OsnovnaPremija, Valuta = p.Valuta,
            NacinPlacanja = p.NacinPlacanja, AgentId = p.Agent?.OsobljeId,
            AgentIme = p.Agent != null ? $"{p.Agent.Ime} {p.Agent.Prezime}" : null,
            DatumIsteka = p.DatumIsteka
        };

        // Poredi staru i novu listu ID-jeva povezanih entiteta (npr. nekretnine na Imovinskom
        // osiguranju) i vraca citljiv opis sta je dodato/uklonjeno, ili null ako nema promene.
        private static string? DiffPovezanihEntiteta<T>(ISession s, string naziv, IList<int> stareIds, IList<int> noveIds) where T : class
        {
            var staroSet = stareIds.ToHashSet();
            var novoSet = noveIds.ToHashSet();
            var dodatiIds = novoSet.Except(staroSet).ToList();
            var uklonjeniIds = staroSet.Except(novoSet).ToList();
            if (dodatiIds.Count == 0 && uklonjeniIds.Count == 0) return null;

            var delovi = new List<string>();
            if (dodatiIds.Count > 0)
                delovi.Add("dodato: " + string.Join(", ", dodatiIds.Select(id => s.Get<T>(id)?.ToString() ?? $"#{id}")));
            if (uklonjeniIds.Count > 0)
                delovi.Add("uklonjeno: " + string.Join(", ", uklonjeniIds.Select(id => s.Get<T>(id)?.ToString() ?? $"#{id}")));
            return $"{naziv}: {string.Join("; ", delovi)}";
        }

        // Belezi jedan red u ISTORIJA_POLISE pri svakoj izmeni postojece polise (poziva se samo iz
        // azurirajXOsiguranje metoda, ne i iz dodajXOsiguranje - istorija prati promene, ne nastanak).
        private static void ZabelezIstorijuPromene(ISession s, Polisa p, StaroStanjePolise staro, IEnumerable<string>? dodatnePromene = null)
        {
            string tip = (p.Status, staro.Status) switch
            {
                ("RASKINUTA", var stari) when stari != "RASKINUTA" => "RASKID",
                ("MIROVANJE", var stari) when stari != "MIROVANJE" => "MIROVANJE",
                ("OBNOVLJENA", var stari) when stari != "OBNOVLJENA" => "OBNOVA",
                ("AKTIVNA", var stari) when stari != "AKTIVNA" => "REAKTIVACIJA",
                _ => "IZMENA"
            };

            var promene = new List<string>();
            if (staro.Status != p.Status)
                promene.Add($"Status: {staro.Status} → {p.Status}");
            if (staro.OsnovnaPremija != p.OsnovnaPremija || staro.Valuta != p.Valuta)
                promene.Add($"Premija: {staro.OsnovnaPremija:0.##} {staro.Valuta} → {p.OsnovnaPremija:0.##} {p.Valuta}");
            if (staro.NacinPlacanja != p.NacinPlacanja)
                promene.Add($"Način plaćanja: {staro.NacinPlacanja} → {p.NacinPlacanja}");
            if (staro.AgentId != p.Agent?.OsobljeId)
            {
                string noviAgentIme = p.Agent != null ? $"{p.Agent.Ime} {p.Agent.Prezime}" : "/";
                promene.Add($"Agent: {staro.AgentIme ?? "/"} → {noviAgentIme}");
            }
            if (staro.DatumIsteka != p.DatumIsteka)
                promene.Add($"Datum isteka: {staro.DatumIsteka:dd.MM.yyyy} → {p.DatumIsteka:dd.MM.yyyy}");
            if (dodatnePromene != null)
                promene.AddRange(dodatnePromene);

            string opis = promene.Count > 0 ? string.Join("; ", promene) : "Izmena podataka polise";
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

        public static List<IstorijaPoliseBasic> vratiIstorijuPolise(int polisaId)
        {
            var lista = new List<IstorijaPoliseBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var ip in s.Query<IstorijaPolise>().Where(x => x.Polisa!.PolisaId == polisaId).OrderByDescending(x => x.DatumPromene))
                    lista.Add(new IstorijaPoliseBasic
                    {
                        IstorijaId = ip.IstorijaId, PolisaId = polisaId,
                        TipPromene = ip.TipPromene, DatumPromene = ip.DatumPromene, Opis = ip.Opis,
                        KorisnikIme = ip.Korisnik != null ? $"{ip.Korisnik.Ime} {ip.Korisnik.Prezime}" : null
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        // ---------- vratiPolisuDetaljno: puni tip-specificni DTO za citanje/izmenu ----------

        public static object vratiPolisuDetaljno(int id)
        {
            try
            {
                ISession s = DataLayer.GetSession();
                // s.Get (ne s.Load) je neophodan da bi runtime tip bio tacan pre switch/is provere -
                // Load vraca lenj proxy baznog tipa Polisa pa nijedan podtip u switch-u ne bi odgovarao.
                Polisa? p = s.Get<Polisa>(id);
                if (p == null) { s.Close(); MessageBox.Show("Polisa nije pronađena.", "Greška"); return new PolisaBasic(); }
                object dto = p switch
                {
                    ZivotnoOsiguranje z => new ZivotnoPregled
                    {
                        SumaOsiguranja = z.SumaOsiguranja, TipIsplate = z.TipIsplate
                    },
                    ZdravstvenoOsiguranje zd => new ZdravstvenoPregled
                    {
                        MrezaUstanova = zd.MrezaUstanova, Pokrica = zd.Pokrica,
                        LimitSpecijalista = zd.LimitSpecijalista, LimitStomatologa = zd.LimitStomatologa,
                        LimitBolnickih = zd.LimitBolnickih, LimitBolnickiDan = zd.LimitBolnickiDan
                    },
                    AutoOsiguranje a => new AutoPolisaPregled
                    {
                        VoziloId = a.Vozilo?.VoziloId ?? 0, VoziloOpis = a.Vozilo?.ToString(),
                        BonusMalusKlasa = a.BonusMalusKlasa, TeritorijanoVazenje = a.TeritorijanoVazenje
                    },
                    ImovinskOsiguranje im => new ImovinskoPregled
                    {
                        VrsteRizika = im.VrsteRizika,
                        NekretnineIds = im.Nekretnine.Select(n => n.NekretninaId).ToList(),
                        PokretnaImovinaIds = im.PokretneImovine.Select(pi => pi.PokretnaImovinaId).ToList()
                    },
                    PutnoOsiguranje pu => new PutnoPregled
                    {
                        Destinacije = pu.Destinacije, DatumPolaska = pu.DatumPolaska, DatumPovratka = pu.DatumPovratka,
                        OsiguranaLicaIds = pu.OsiguranaLica.Select(k => k.KlijentId).ToList()
                    },
                    PoljoprivrednoOsiguranje polj => new PoljoprivrednoPregled
                    {
                        UseviIds = polj.Usevi.Select(u => u.UsevId).ToList(),
                        ZivotinjeIds = polj.Zivotinje.Select(z => z.ZivotinjaId).ToList()
                    },
                    OsiguranjeOdgovornosti od => new OdgovornostPregled
                    {
                        VrstaOdgovornosti = od.VrstaOdgovornosti, LimitOdgovornosti = od.LimitOdgovornosti
                    },
                    SpecijalizovanoOsiguranje sp => new SpecijalizovanoPregled
                    {
                        NazivSpecijalizacije = sp.NazivSpecijalizacije, OpisUslova = sp.OpisUslova
                    },
                    _ => new PolisaBasic()
                };
                // zajednicka polja (bazni deo) - popunjena kroz refleksiju bi bilo elegantnije,
                // ali eksplicitno je citljivije i izbegava iznenadjenja.
                var bazni = (PolisaPregled)dto;
                bazni.PolisaId = p.PolisaId; bazni.BrojPolise = p.BrojPolise;
                bazni.TipOsiguranja = p.TipOsiguranja;
                bazni.DatumPocetka = p.DatumPocetka; bazni.DatumIsteka = p.DatumIsteka;
                bazni.Status = p.Status; bazni.OsnovnaPremija = p.OsnovnaPremija;
                bazni.Valuta = p.Valuta; bazni.NacinPlacanja = p.NacinPlacanja;
                bazni.UgovaracId = p.Ugovarac?.KlijentId ?? 0; bazni.UgovaracNaziv = p.Ugovarac?.Naziv;
                bazni.AgentId = p.Agent?.OsobljeId;
                bazni.AgentIme = p.Agent != null ? $"{p.Agent.Ime} {p.Agent.Prezime}" : null;
                foreach (var dp in p.DodatnaPokrića)
                    bazni.DodatnaPokrića.Add(new DodatnoPokrBasic
                    {
                        PokriceId = dp.PokriceId, PolisaId = p.PolisaId,
                        Naziv = dp.Naziv, Opis = dp.Opis,
                        LimitPokrića = dp.LimitPokrića, Fransiza = dp.Fransiza, DodatnaPremija = dp.DodatnaPremija
                    });
                s.Close();
                return dto;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return new PolisaBasic(); }
        }

        // ---------- ZivotnoOsiguranje ----------

        public static int dodajZivotnoOsiguranje(ZivotnoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var z = new ZivotnoOsiguranje { TipOsiguranja = "ZIVOTNO", SumaOsiguranja = dto.SumaOsiguranja, TipIsplate = dto.TipIsplate };
                popuniBazuPolise(z, dto, s);
                s.Save(z);
                s.Flush();
                s.Close();
                return z.PolisaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajZivotnoOsiguranje(ZivotnoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                ZivotnoOsiguranje z = s.Load<ZivotnoOsiguranje>(dto.PolisaId);
                var staro = UhvatiStaroStanjePolise(z);
                popuniBazuPolise(z, dto, s);
                z.SumaOsiguranja = dto.SumaOsiguranja; z.TipIsplate = dto.TipIsplate;
                ZabelezIstorijuPromene(s, z, staro);
                s.SaveOrUpdate(z);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- ZdravstvenoOsiguranje ----------

        public static int dodajZdravstvenoOsiguranje(ZdravstvenoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var z = new ZdravstvenoOsiguranje
                {
                    TipOsiguranja = "ZDRAVSTVENO", MrezaUstanova = dto.MrezaUstanova, Pokrica = dto.Pokrica,
                    LimitSpecijalista = dto.LimitSpecijalista, LimitStomatologa = dto.LimitStomatologa,
                    LimitBolnickih = dto.LimitBolnickih, LimitBolnickiDan = dto.LimitBolnickiDan
                };
                popuniBazuPolise(z, dto, s);
                s.Save(z);
                s.Flush();
                s.Close();
                return z.PolisaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajZdravstvenoOsiguranje(ZdravstvenoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                ZdravstvenoOsiguranje z = s.Load<ZdravstvenoOsiguranje>(dto.PolisaId);
                var staro = UhvatiStaroStanjePolise(z);
                popuniBazuPolise(z, dto, s);
                z.MrezaUstanova = dto.MrezaUstanova; z.Pokrica = dto.Pokrica;
                z.LimitSpecijalista = dto.LimitSpecijalista; z.LimitStomatologa = dto.LimitStomatologa;
                z.LimitBolnickih = dto.LimitBolnickih; z.LimitBolnickiDan = dto.LimitBolnickiDan;
                ZabelezIstorijuPromene(s, z, staro);
                s.SaveOrUpdate(z);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- AutoOsiguranje ----------

        public static int dodajAutoOsiguranje(AutoPolisaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var a = new AutoOsiguranje
                {
                    TipOsiguranja = "AUTO",
                    Vozilo = dto.VoziloId > 0 ? s.Load<Vozilo>(dto.VoziloId) : null,
                    BonusMalusKlasa = dto.BonusMalusKlasa, TeritorijanoVazenje = dto.TeritorijanoVazenje
                };
                popuniBazuPolise(a, dto, s);
                s.Save(a);
                s.Flush();
                s.Close();
                return a.PolisaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajAutoOsiguranje(AutoPolisaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                AutoOsiguranje a = s.Load<AutoOsiguranje>(dto.PolisaId);
                var staro = UhvatiStaroStanjePolise(a);
                string? staroVozilo = a.Vozilo?.ToString();
                popuniBazuPolise(a, dto, s);
                a.Vozilo = dto.VoziloId > 0 ? s.Load<Vozilo>(dto.VoziloId) : null;
                a.BonusMalusKlasa = dto.BonusMalusKlasa; a.TeritorijanoVazenje = dto.TeritorijanoVazenje;
                var dodatnePromene = new List<string>();
                if (staroVozilo != a.Vozilo?.ToString())
                    dodatnePromene.Add($"Vozilo: {staroVozilo ?? "/"} → {a.Vozilo?.ToString() ?? "/"}");
                ZabelezIstorijuPromene(s, a, staro, dodatnePromene);
                s.SaveOrUpdate(a);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // Auto - vozaci (dodatni vozaci van vlasnika, isto kao web AutoOsiguranje.Vozaci)
        public static List<int> vratiVozaceAutoOsiguranja(int polisaId)
        {
            var lista = new List<int>();
            try
            {
                ISession s = DataLayer.GetSession();
                AutoOsiguranje a = s.Load<AutoOsiguranje>(polisaId);
                lista = a.Vozaci.Select(k => k.KlijentId).ToList();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void azurirajVozaceAutoOsiguranja(int polisaId, List<int> vozaciIds)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                AutoOsiguranje a = s.Load<AutoOsiguranje>(polisaId);
                a.Vozaci.Clear();
                foreach (var id in vozaciIds) a.Vozaci.Add(s.Load<Klijent>(id));
                s.SaveOrUpdate(a);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- ImovinskOsiguranje ----------

        public static int dodajImovinskoOsiguranje(ImovinskoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var im = new ImovinskOsiguranje { TipOsiguranja = "IMOVINSKO", VrsteRizika = dto.VrsteRizika };
                popuniBazuPolise(im, dto, s);
                foreach (var id in dto.NekretnineIds) im.Nekretnine.Add(s.Load<Nekretnina>(id));
                foreach (var id in dto.PokretnaImovinaIds) im.PokretneImovine.Add(s.Load<PokretnaImovina>(id));
                s.Save(im);
                s.Flush();
                s.Close();
                return im.PolisaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajImovinskoOsiguranje(ImovinskoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                ImovinskOsiguranje im = s.Load<ImovinskOsiguranje>(dto.PolisaId);
                var staro = UhvatiStaroStanjePolise(im);
                var staroNekretnineIds = im.Nekretnine.Select(n => n.NekretninaId).ToList();
                var staroPokretneImovineIds = im.PokretneImovine.Select(pi => pi.PokretnaImovinaId).ToList();
                popuniBazuPolise(im, dto, s);
                im.VrsteRizika = dto.VrsteRizika;
                im.Nekretnine.Clear();
                foreach (var id in dto.NekretnineIds) im.Nekretnine.Add(s.Load<Nekretnina>(id));
                im.PokretneImovine.Clear();
                foreach (var id in dto.PokretnaImovinaIds) im.PokretneImovine.Add(s.Load<PokretnaImovina>(id));
                var dodatnePromene = new List<string>();
                var nekretnineDiff = DiffPovezanihEntiteta<Nekretnina>(s, "Nekretnine", staroNekretnineIds, dto.NekretnineIds);
                if (nekretnineDiff != null) dodatnePromene.Add(nekretnineDiff);
                var pokretnaDiff = DiffPovezanihEntiteta<PokretnaImovina>(s, "Pokretna imovina", staroPokretneImovineIds, dto.PokretnaImovinaIds);
                if (pokretnaDiff != null) dodatnePromene.Add(pokretnaDiff);
                ZabelezIstorijuPromene(s, im, staro, dodatnePromene);
                s.SaveOrUpdate(im);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- PutnoOsiguranje ----------

        public static int dodajPutnoOsiguranje(PutnoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var pu = new PutnoOsiguranje
                {
                    TipOsiguranja = "PUTNO", Destinacije = dto.Destinacije,
                    DatumPolaska = dto.DatumPolaska, DatumPovratka = dto.DatumPovratka
                };
                popuniBazuPolise(pu, dto, s);
                foreach (var id in dto.OsiguranaLicaIds) pu.OsiguranaLica.Add(s.Load<Klijent>(id));
                s.Save(pu);
                s.Flush();
                s.Close();
                return pu.PolisaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajPutnoOsiguranje(PutnoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                PutnoOsiguranje pu = s.Load<PutnoOsiguranje>(dto.PolisaId);
                var staro = UhvatiStaroStanjePolise(pu);
                var staroOsiguranaLicaIds = pu.OsiguranaLica.Select(k => k.KlijentId).ToList();
                popuniBazuPolise(pu, dto, s);
                pu.Destinacije = dto.Destinacije; pu.DatumPolaska = dto.DatumPolaska; pu.DatumPovratka = dto.DatumPovratka;
                pu.OsiguranaLica.Clear();
                foreach (var id in dto.OsiguranaLicaIds) pu.OsiguranaLica.Add(s.Load<Klijent>(id));
                var dodatnePromene = new List<string>();
                var licaDiff = DiffPovezanihEntiteta<Klijent>(s, "Osigurana lica", staroOsiguranaLicaIds, dto.OsiguranaLicaIds);
                if (licaDiff != null) dodatnePromene.Add(licaDiff);
                ZabelezIstorijuPromene(s, pu, staro, dodatnePromene);
                s.SaveOrUpdate(pu);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Osiguranici (UlogaKlijenta) - klijent registrovan ili slobodan unos imena ----------

        public static List<UlogaKlijentaBasic> vratiUlogeZaPolisu(int polisaId)
        {
            var lista = new List<UlogaKlijentaBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var u in s.Query<UlogaKlijenta>().Where(x => x.Polisa!.PolisaId == polisaId))
                    lista.Add(new UlogaKlijentaBasic
                    {
                        UlogaId = u.UlogaId, PolisaId = polisaId,
                        KlijentId = u.Klijent?.KlijentId, KlijentNaziv = u.Klijent?.Naziv,
                        ImePrezime = u.ImePrezime, TipUloge = u.TipUloge
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
                    Polisa = s.Load<Polisa>(dto.PolisaId),
                    Klijent = dto.KlijentId.HasValue ? s.Load<Klijent>(dto.KlijentId.Value) : null,
                    ImePrezime = dto.ImePrezime,
                    TipUloge = dto.TipUloge ?? "OSIGURANIK"
                };
                s.Save(u);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Dodatna pokrića ----------

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
                    LimitPokrića = dto.LimitPokrića, Fransiza = dto.Fransiza, DodatnaPremija = dto.DodatnaPremija
                };
                s.Save(dp);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Korisnici isplate (samo ZivotnoOsiguranje) ----------

        public static List<KorisnikIsplateBasic> vratiKorisnikeIsplateZaPolisu(int polisaId)
        {
            var lista = new List<KorisnikIsplateBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var k in s.Query<KorisnikIsplate>().Where(x => x.Polisa!.PolisaId == polisaId))
                    lista.Add(new KorisnikIsplateBasic
                    {
                        KorisnikId = k.KorisnikId, PolisaId = polisaId,
                        KlijentId = k.Klijent?.KlijentId, KlijentNaziv = k.Klijent?.Naziv,
                        ImePrezime = k.ImePrezime, ProcenatUdela = k.ProcenatUdela
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }
    }
}
