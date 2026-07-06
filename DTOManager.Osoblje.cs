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

        public static List<OsobljePregled> vratiSveOsoblje(string? tip = null)
        {
            var lista = new List<OsobljePregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                var q = s.Query<Osoblje>().AsQueryable();
                if (!string.IsNullOrEmpty(tip) && tip != "SVE")
                    q = q.Where(o => o.TipOsoblja == tip);
                foreach (var o in q.OrderBy(o => o.Prezime).ThenBy(o => o.Ime))
                    lista.Add(mapOsobljePregled(o));
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static List<AgentBasic> vratiSveAgente()
        {
            var lista = new List<AgentBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var a in s.Query<Agent>().OrderBy(a => a.Prezime))
                {
                    var ab = new AgentBasic
                    {
                        OsobljeId = a.OsobljeId, Ime = a.Ime, Prezime = a.Prezime,
                        TipOsoblja = "AGENT", Status = a.Status,
                        Telefon = a.Telefon, Email = a.Email,
                        DatumAngazovanja = a.DatumAngazovanja,
                        TipAgenta = a.TipAgenta, Licenca = a.Licenca,
                        RegionRada = a.RegionRada, ProvizijaProcenat = a.ProvizijaProcenat
                    };
                    foreach (var p in a.Polise)
                        ab.Polise.Add(mapPolisaPregled(p));
                    lista.Add(ab);
                }
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static List<ProceniteljBasic> vratiSveProcenitelje()
        {
            var lista = new List<ProceniteljBasic>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var p in s.Query<Procenitelj>().OrderBy(p => p.Prezime))
                {
                    var pb = new ProceniteljBasic
                    {
                        OsobljeId = p.OsobljeId, Ime = p.Ime, Prezime = p.Prezime,
                        TipOsoblja = "PROCENITELJ", Status = p.Status,
                        Telefon = p.Telefon, Email = p.Email,
                        DatumAngazovanja = p.DatumAngazovanja
                    };
                    foreach (var ob in p.OblasiProc)
                        if (ob.Oblast != null) pb.Oblasti.Add(ob.Oblast);
                    lista.Add(pb);
                }
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void dodajAgenta(AgentBasic dto)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                var a = new Agent
                {
                    Ime = dto.Ime, Prezime = dto.Prezime, Jmbg = dto.Jmbg,
                    Adresa = dto.Adresa, Telefon = dto.Telefon, Email = dto.Email,
                    DatumAngazovanja = DateTime.Today,
                    Status = dto.Status ?? "AKTIVAN",
                    TipOsoblja = "AGENT",
                    TipAgenta = dto.TipAgenta,
                    Licenca = dto.Licenca,
                    RegionRada = dto.RegionRada,
                    ProvizijaProcenat = dto.ProvizijaProcenat
                };
                s.Save(a);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void dodajOsoblje(OsobljeBasic dto)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Osoblje o;
                switch (dto.TipOsoblja)
                {
                    case "PROCENITELJ": o = new Procenitelj(); break;
                    case "LEKAR":       o = new Lekar();       break;
                    case "PRAVNIK":
                        o = new Pravnik { TipPravnika = dto.TipPravnika, BarBroj = dto.BarBroj };
                        break;
                    default: o = new Osoblje(); break;
                }
                o.Ime = dto.Ime; o.Prezime = dto.Prezime; o.Jmbg = dto.Jmbg;
                o.Adresa = dto.Adresa; o.Telefon = dto.Telefon; o.Email = dto.Email;
                o.DatumAngazovanja = DateTime.Today;
                o.Status = dto.Status ?? "AKTIVAN";
                o.TipOsoblja = dto.TipOsoblja;
                s.Save(o);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void azurirajOsoblje(OsobljeBasic dto)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Osoblje  o = s.Load<Osoblje>(dto.OsobljeId);
                o.Ime = dto.Ime; o.Prezime = dto.Prezime;
                o.Adresa = dto.Adresa; o.Telefon = dto.Telefon;
                o.Email = dto.Email; o.Status = dto.Status;
                s.SaveOrUpdate(o);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiOsoblje(int id)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Osoblje  o = s.Load<Osoblje>(id);
                s.Delete(o);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        private static OsobljePregled mapOsobljePregled(Osoblje o)
        {
            if (o is Agent a)
                return new AgentBasic
                {
                    OsobljeId = a.OsobljeId, Ime = a.Ime, Prezime = a.Prezime,
                    TipOsoblja = "AGENT", Status = a.Status,
                    Telefon = a.Telefon, Email = a.Email,
                    DatumAngazovanja = a.DatumAngazovanja,
                    TipAgenta = a.TipAgenta, Licenca = a.Licenca,
                    RegionRada = a.RegionRada, ProvizijaProcenat = a.ProvizijaProcenat
                };
            return new OsobljePregled
            {
                OsobljeId = o.OsobljeId, Ime = o.Ime, Prezime = o.Prezime,
                TipOsoblja = o.TipOsoblja, Status = o.Status,
                Telefon = o.Telefon, Email = o.Email,
                DatumAngazovanja = o.DatumAngazovanja
            };
        }
    }
}
