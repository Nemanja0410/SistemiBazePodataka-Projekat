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
    // Entiteti dodati naknadno da bi se pokrile praznine u odnosu na tekst zadatka 18:
    // Usev, Zivotinja, PokretnaImovina (predmeti osiguranja), Fotografija (uz stetu)
    // i podtipovi polise Poljoprivredno/Odgovornost/Specijalizovano osiguranje.
    public partial class DTOManager
    {
        // ---------- Usev ----------

        public static List<UsevBasic> vratiSveUseve()
        {
            var lista = new List<UsevBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var u in s.Query<Usev>().OrderBy(x => x.Vrsta))
                    lista.Add(new UsevBasic
                    {
                        UsevId = u.UsevId, Vrsta = u.Vrsta,
                        Lokacija = u.Lokacija, ProcenjenaVrednost = u.ProcenjenaVrednost
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void dodajUsev(UsevBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var u = new Usev
                {
                    Vrsta = dto.Vrsta, Lokacija = dto.Lokacija,
                    ProcenjenaVrednost = dto.ProcenjenaVrednost
                };
                s.Save(u);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void azurirajUsev(UsevBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                Usev u = s.Load<Usev>(dto.UsevId);
                u.Vrsta = dto.Vrsta; u.Lokacija = dto.Lokacija;
                u.ProcenjenaVrednost = dto.ProcenjenaVrednost;
                s.SaveOrUpdate(u);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiUsev(int id)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Usev u = s.Load<Usev>(id);
                s.Delete(u);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Zivotinja ----------

        public static List<ZivotinjaBasic> vratiSveZivotinje()
        {
            var lista = new List<ZivotinjaBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var z in s.Query<Zivotinja>().OrderBy(x => x.Vrsta))
                    lista.Add(new ZivotinjaBasic
                    {
                        ZivotinjaId = z.ZivotinjaId, Vrsta = z.Vrsta,
                        Lokacija = z.Lokacija, ProcenjenaVrednost = z.ProcenjenaVrednost
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void dodajZivotinju(ZivotinjaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var z = new Zivotinja
                {
                    Vrsta = dto.Vrsta, Lokacija = dto.Lokacija,
                    ProcenjenaVrednost = dto.ProcenjenaVrednost
                };
                s.Save(z);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void azurirajZivotinju(ZivotinjaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                Zivotinja z = s.Load<Zivotinja>(dto.ZivotinjaId);
                z.Vrsta = dto.Vrsta; z.Lokacija = dto.Lokacija;
                z.ProcenjenaVrednost = dto.ProcenjenaVrednost;
                s.SaveOrUpdate(z);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiZivotinju(int id)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Zivotinja z = s.Load<Zivotinja>(id);
                s.Delete(z);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- PokretnaImovina ----------

        public static List<PokretnaImovinaBasic> vratiSvuPokretnuImovinu()
        {
            var lista = new List<PokretnaImovinaBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var p in s.Query<PokretnaImovina>().OrderBy(x => x.Naziv))
                    lista.Add(new PokretnaImovinaBasic
                    {
                        PokretnaImovinaId = p.PokretnaImovinaId, Naziv = p.Naziv,
                        Opis = p.Opis, ProcenjenaVrednost = p.ProcenjenaVrednost
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void dodajPokretnuImovinu(PokretnaImovinaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var p = new PokretnaImovina
                {
                    Naziv = dto.Naziv, Opis = dto.Opis,
                    ProcenjenaVrednost = dto.ProcenjenaVrednost
                };
                s.Save(p);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void azurirajPokretnuImovinu(PokretnaImovinaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                PokretnaImovina p = s.Load<PokretnaImovina>(dto.PokretnaImovinaId);
                p.Naziv = dto.Naziv; p.Opis = dto.Opis;
                p.ProcenjenaVrednost = dto.ProcenjenaVrednost;
                s.SaveOrUpdate(p);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiPokretnuImovinu(int id)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                PokretnaImovina p = s.Load<PokretnaImovina>(id);
                s.Delete(p);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Nekretnina ----------

        public static List<NekretninaBasic> vratiSveNekretnine()
        {
            var lista = new List<NekretninaBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var n in s.Query<Nekretnina>().OrderBy(x => x.Adresa))
                    lista.Add(new NekretninaBasic
                    {
                        NekretninaId = n.NekretninaId, Adresa = n.Adresa, TipObjekta = n.TipObjekta,
                        Povrsina = n.Povrsina, GodinaIzgradnje = n.GodinaIzgradnje,
                        ProcenjenaVrednost = n.ProcenjenaVrednost
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Vozilo (azuriraj/obrisi - dodaj/vrati postoje u DTOManager.Stete.cs) ----------

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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Fotografija (uz stetu) ----------

        public static List<FotografijaBasic> vratiFotografijeZaStetu(int stetaId)
        {
            var lista = new List<FotografijaBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var f in s.Query<Fotografija>().Where(x => x.Steta!.StetaId == stetaId))
                    lista.Add(new FotografijaBasic
                    {
                        FotografijaId = f.FotografijaId, StetaId = stetaId,
                        Putanja = f.Putanja, Opis = f.Opis, DatumDodavanja = f.DatumDodavanja
                    });
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void dodajFotografiju(FotografijaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");
            try
            {
                ISession s = DataLayer.GetSession();
                var f = new Fotografija
                {
                    Steta = s.Load<Steta>(dto.StetaId),
                    Putanja = dto.Putanja, Opis = dto.Opis,
                    DatumDodavanja = DateTime.Now
                };
                s.Save(f);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiFotografiju(int id)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                Fotografija f = s.Load<Fotografija>(id);
                s.Delete(f);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---------- Novi podtipovi polise ----------

        private static void popuniBazuPolise(Polisa p, PolisaPregled dto, ISession s)
        {
            p.BrojPolise = dto.BrojPolise;
            if (p.PolisaId == 0) p.DatumZakljucenja = DateTime.Today; // samo pri kreiranju, ne pri izmeni
            p.DatumPocetka = dto.DatumPocetka;
            p.DatumIsteka = dto.DatumIsteka;
            p.Status = dto.Status ?? "AKTIVNA";
            p.OsnovnaPremija = dto.OsnovnaPremija;
            p.Valuta = dto.Valuta ?? "RSD";
            p.NacinPlacanja = dto.NacinPlacanja ?? "MESECNO";
            p.Ugovarac = s.Load<Klijent>(dto.UgovaracId);
            p.Agent = dto.AgentId.HasValue ? s.Load<Agent>(dto.AgentId.Value) : null;
        }

        public static int dodajPoljoprivrednoOsiguranje(PoljoprivrednoPregled dto)
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
                return p.PolisaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajPoljoprivrednoOsiguranje(PoljoprivrednoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                PoljoprivrednoOsiguranje p = s.Load<PoljoprivrednoOsiguranje>(dto.PolisaId);
                var staro = UhvatiStaroStanjePolise(p);
                var staroUseviIds = p.Usevi.Select(u => u.UsevId).ToList();
                var staroZivotinjeIds = p.Zivotinje.Select(z => z.ZivotinjaId).ToList();
                popuniBazuPolise(p, dto, s);
                p.Usevi.Clear();
                foreach (var usevId in dto.UseviIds) p.Usevi.Add(s.Load<Usev>(usevId));
                p.Zivotinje.Clear();
                foreach (var zivotinjaId in dto.ZivotinjeIds) p.Zivotinje.Add(s.Load<Zivotinja>(zivotinjaId));
                var dodatnePromene = new List<string>();
                var useviDiff = DiffPovezanihEntiteta<Usev>(s, "Usevi", staroUseviIds, dto.UseviIds);
                if (useviDiff != null) dodatnePromene.Add(useviDiff);
                var zivotinjeDiff = DiffPovezanihEntiteta<Zivotinja>(s, "Životinje", staroZivotinjeIds, dto.ZivotinjeIds);
                if (zivotinjeDiff != null) dodatnePromene.Add(zivotinjeDiff);
                ZabelezIstorijuPromene(s, p, staro, dodatnePromene);
                s.SaveOrUpdate(p);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static int dodajOsiguranjeOdgovornosti(OdgovornostPregled dto)
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
                return o.PolisaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajOsiguranjeOdgovornosti(OdgovornostPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                OsiguranjeOdgovornosti o = s.Load<OsiguranjeOdgovornosti>(dto.PolisaId);
                var staro = UhvatiStaroStanjePolise(o);
                popuniBazuPolise(o, dto, s);
                o.VrstaOdgovornosti = dto.VrstaOdgovornosti;
                o.LimitOdgovornosti = dto.LimitOdgovornosti;
                ZabelezIstorijuPromene(s, o, staro);
                s.SaveOrUpdate(o);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static int dodajSpecijalizovanoOsiguranje(SpecijalizovanoPregled dto)
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
                return sp.PolisaId;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); return 0; }
        }

        public static void azurirajSpecijalizovanoOsiguranje(SpecijalizovanoPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                SpecijalizovanoOsiguranje sp = s.Load<SpecijalizovanoOsiguranje>(dto.PolisaId);
                var staro = UhvatiStaroStanjePolise(sp);
                popuniBazuPolise(sp, dto, s);
                sp.NazivSpecijalizacije = dto.NazivSpecijalizacije;
                sp.OpisUslova = dto.OpisUslova;
                ZabelezIstorijuPromene(s, sp, staro);
                s.SaveOrUpdate(sp);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }
    }
}
