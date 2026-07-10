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
            p.DatumZakljucenja = DateTime.Today;
            p.DatumPocetka = dto.DatumPocetka;
            p.DatumIsteka = dto.DatumIsteka;
            p.Status = dto.Status ?? "AKTIVNA";
            p.OsnovnaPremija = dto.OsnovnaPremija;
            p.Valuta = dto.Valuta ?? "RSD";
            p.NacinPlacanja = dto.NacinPlacanja ?? "MESECNO";
            p.Ugovarac = s.Load<Klijent>(dto.UgovaracId);
            p.Agent = dto.AgentId.HasValue ? s.Load<Agent>(dto.AgentId.Value) : null;
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
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
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }
    }
}
