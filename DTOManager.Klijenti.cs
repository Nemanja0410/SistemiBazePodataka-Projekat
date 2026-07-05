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
        // ============================================================
        //  KLIJENTI
        // ============================================================

        public static List<KlijentPregled> vratiSveKlijente()
        {
            var lista = new List<KlijentPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var k in s.Query<Klijent>().OrderBy(k => k.Naziv))
                    lista.Add(mapKlijentPregled(k));
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static List<KlijentPregled> pretraziKlijente(string naziv, string? tip)
        {
            var lista = new List<KlijentPregled>();
            try
            {
                ISession s  = DataLayer.GetSession();
                var q = s.Query<Klijent>().AsQueryable();
                if (!string.IsNullOrEmpty(naziv))
                    q = q.Where(k => (k.Naziv ?? "").ToUpper().Contains(naziv.ToUpper()));
                if (!string.IsNullOrEmpty(tip) && tip != "SVE")
                    q = q.Where(k => k.TipKlijenta == tip);
                foreach (var k in q.OrderBy(k => k.Naziv))
                    lista.Add(mapKlijentPregled(k));
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static KlijentBasic vratiKlijenta(int id)
        {
            var dto = new KlijentBasic();
            try
            {
                ISession s = DataLayer.GetSession();
                Klijent  k = s.Load<Klijent>(id);
                dto = new KlijentBasic
                {
                    KlijentId = k.KlijentId, Naziv = k.Naziv,
                    TipKlijenta = k.TipKlijenta, Adresa = k.Adresa,
                    Telefon = k.Telefon, Email = k.Email,
                    Status = k.Status, DatumRegistracije = k.DatumRegistracije
                };
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return dto;
        }

        public static void dodajFizickoLice(FizickoLiceBasic dto)
        {
            try
            {
                ISession s = DataLayer.GetSession();
                var fl = new FizickoLice
                {
                    Naziv = dto.Naziv, Adresa = dto.Adresa,
                    Telefon = dto.Telefon, Email = dto.Email,
                    Status = dto.Status ?? "AKTIVAN",
                    DatumRegistracije = DateTime.Today,
                    TipKlijenta = "FIZICKO_LICE",
                    Jmbg = dto.Jmbg,
                    DatumRodjenja = dto.DatumRodjenja,
                    Zanimanje = dto.Zanimanje
                };
                s.Save(fl);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void dodajPravnoLice(PravnoLiceBasic dto)
        {
            try
            {
                ISession s = DataLayer.GetSession();
                var pl = new PravnoLice
                {
                    Naziv = dto.Naziv, Adresa = dto.Adresa,
                    Telefon = dto.Telefon, Email = dto.Email,
                    Status = dto.Status ?? "AKTIVAN",
                    DatumRegistracije = DateTime.Today,
                    TipKlijenta = "PRAVNO_LICE",
                    Pib = dto.Pib, MaticniBroj = dto.MaticniBroj,
                    Delatnost = dto.Delatnost
                };
                s.Save(pl);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void dodajJavnuInstituciju(JavnaInstitucijaBasic dto)
        {
            try
            {
                ISession s = DataLayer.GetSession();
                var ji = new JavnaInstitucija
                {
                    Naziv = dto.Naziv, Adresa = dto.Adresa,
                    Telefon = dto.Telefon, Email = dto.Email,
                    Status = dto.Status ?? "AKTIVAN",
                    DatumRegistracije = DateTime.Today,
                    TipKlijenta = "JAVNA_INSTITUCIJA",
                    Pib = dto.Pib, MaticniBroj = dto.MaticniBroj,
                    Delatnost = dto.Delatnost,
                    NivoInstitucije = dto.NivoInstitucije
                };
                s.Save(ji);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void azurirajKlijenta(KlijentBasic dto)
        {
            try
            {
                ISession s = DataLayer.GetSession();
                Klijent  k = s.Load<Klijent>(dto.KlijentId);
                k.Naziv   = dto.Naziv;   k.Adresa  = dto.Adresa;
                k.Telefon = dto.Telefon; k.Email   = dto.Email;
                k.Status  = dto.Status;
                s.SaveOrUpdate(k);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiKlijenta(int id)
        {
            try
            {
                ISession s = DataLayer.GetSession();
                Klijent  k = s.Load<Klijent>(id);
                s.Delete(k);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        // ---- privatni helper ----
        private static KlijentPregled mapKlijentPregled(Klijent k)
        {
            if (k is FizickoLice fl)
                return new FizickoLicePregled
                {
                    KlijentId = k.KlijentId, Naziv = k.Naziv, TipKlijenta = k.TipKlijenta,
                    Adresa = k.Adresa, Telefon = k.Telefon, Email = k.Email,
                    Status = k.Status, DatumRegistracije = k.DatumRegistracije,
                    Jmbg = fl.Jmbg, DatumRodjenja = fl.DatumRodjenja, Zanimanje = fl.Zanimanje
                };
            if (k is PravnoLice pl)
                return new PravnoLicePregled
                {
                    KlijentId = k.KlijentId, Naziv = k.Naziv, TipKlijenta = k.TipKlijenta,
                    Adresa = k.Adresa, Telefon = k.Telefon, Email = k.Email,
                    Status = k.Status, DatumRegistracije = k.DatumRegistracije,
                    Pib = pl.Pib, MaticniBroj = pl.MaticniBroj, Delatnost = pl.Delatnost
                };
            if (k is JavnaInstitucija ji)
                return new JavnaInstitucijaPregled
                {
                    KlijentId = k.KlijentId, Naziv = k.Naziv, TipKlijenta = k.TipKlijenta,
                    Adresa = k.Adresa, Telefon = k.Telefon, Email = k.Email,
                    Status = k.Status, DatumRegistracije = k.DatumRegistracije,
                    Pib = ji.Pib, MaticniBroj = ji.MaticniBroj,
                    Delatnost = ji.Delatnost, NivoInstitucije = ji.NivoInstitucije
                };
            return new KlijentPregled
            {
                KlijentId = k.KlijentId, Naziv = k.Naziv, TipKlijenta = k.TipKlijenta,
                Adresa = k.Adresa, Telefon = k.Telefon, Email = k.Email,
                Status = k.Status, DatumRegistracije = k.DatumRegistracije
            };
        }
    }
}
