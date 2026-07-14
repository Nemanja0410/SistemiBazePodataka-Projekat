using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using OsiguranjApp.DTOs;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp
{
    public partial class DTOManager
    {

        public static List<PolisaPregled> vratiSvePolise(string? tip = null, string? status = null)
        {
            var lista = new List<PolisaPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                var q = s.Query<Polisa>().AsQueryable();
                if (!string.IsNullOrEmpty(tip)    && tip    != "SVE") q = q.Where(p => p.TipOsiguranja == tip);
                if (!string.IsNullOrEmpty(status) && status != "SVE") q = q.Where(p => p.Status == status);
                foreach (var p in q.OrderByDescending(p => p.DatumZakljucenja))
                    lista.Add(mapPolisaPregled(p));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static PolisaBasic vratiPolisu(int id)
        {
            var dto = new PolisaBasic();
            try
            {
                ISession s = DataLayer.GetSession();
                Polisa   p = s.Load<Polisa>(id);
                dto = new PolisaBasic
                {
                    PolisaId = p.PolisaId, BrojPolise = p.BrojPolise,
                    TipOsiguranja = p.TipOsiguranja,
                    DatumPocetka = p.DatumPocetka, DatumIsteka = p.DatumIsteka,
                    Status = p.Status, OsnovnaPremija = p.OsnovnaPremija,
                    Valuta = p.Valuta, NacinPlacanja = p.NacinPlacanja,
                    UgovaracId = p.Ugovarac?.KlijentId ?? 0, UgovaracNaziv = p.Ugovarac?.Naziv,
                    AgentId  = p.Agent?.OsobljeId,
                    AgentIme = p.Agent != null ? $"{p.Agent.Ime} {p.Agent.Prezime}" : null
                };
                foreach (var dp in p.DodatnaPokrića)
                    dto.DodatnaPokrića.Add(new DodatnoPokrBasic
                    {
                        PokriceId = dp.PokriceId, PolisaId = p.PolisaId,
                        Naziv = dp.Naziv, Opis = dp.Opis,
                        LimitPokrića = dp.LimitPokrića, Fransiza = dp.Fransiza,
                        DodatnaPremija = dp.DodatnaPremija
                    });
                s.Close();
            }
            catch (Exception) { throw; }
            return dto;
        }

        // vratiPolisuDetaljno: isto kao vratiPolisu, ali vraca konkretan podtip (AutoPolisaPregled,
        // ZivotnoPregled itd.) sa svim tip-specificnim poljima, ne samo bazna. Get (ne Load) je
        // neophodan da bi runtime tip bio tacan pre "is"/switch provere (isti razlog kao kod
        // vratiKlijentaDetaljno). Povratni tip je object da System.Text.Json serijalizuje po
        // stvarnom (podklasnom) tipu, ne po deklarisanom.
        public static object vratiPolisuDetaljno(int id)
        {
            ISession s = DataLayer.GetSession();
            Polisa? p = s.Get<Polisa>(id);
            if (p == null) { s.Close(); throw new NHibernate.ObjectNotFoundException(id, typeof(Polisa).Name); }

            PolisaPregled rezultat = p switch
            {
                AutoOsiguranje a => mapAutoPolisaPregled(a),
                ZivotnoOsiguranje z => mapZivotnoPregled(z),
                ZdravstvenoOsiguranje zd => mapZdravstvenoPregled(zd),
                PutnoOsiguranje pu => mapPutnoPregled(pu),
                ImovinskOsiguranje im => mapImovinskoPregled(im),
                PoljoprivrednoOsiguranje polj => mapPoljoprivrednoPregled(polj),
                OsiguranjeOdgovornosti od => mapOdgovornostPregled(od),
                SpecijalizovanoOsiguranje sp => mapSpecijalizovanoPregled(sp),
                _ => mapPolisaPregled(p)
            };

            foreach (var dp in p.DodatnaPokrića)
                rezultat.DodatnaPokrića.Add(new DodatnoPokrBasic
                {
                    PokriceId = dp.PokriceId, PolisaId = p.PolisaId,
                    Naziv = dp.Naziv, Opis = dp.Opis,
                    LimitPokrića = dp.LimitPokrića, Fransiza = dp.Fransiza,
                    DodatnaPremija = dp.DodatnaPremija
                });

            s.Close();
            return rezultat;
        }

        public static void dodajPolisu(PolisaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var p = new Polisa
                {
                    BrojPolise = dto.BrojPolise, TipOsiguranja = dto.TipOsiguranja,
                    DatumZakljucenja = DateTime.Today,
                    DatumPocetka = dto.DatumPocetka, DatumIsteka = dto.DatumIsteka,
                    Status = dto.Status ?? "AKTIVNA",
                    OsnovnaPremija = dto.OsnovnaPremija,
                    Valuta = dto.Valuta ?? "RSD",
                    NacinPlacanja = dto.NacinPlacanja ?? "MESECNO",
                    Ugovarac = s.Load<Klijent>(dto.UgovaracId),
                    Agent = dto.AgentId.HasValue ? s.Load<Agent>(dto.AgentId.Value) : null
                };
                s.Save(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajPolisu(PolisaBasic dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                Polisa   p = s.Load<Polisa>(dto.PolisaId);
                p.BrojPolise = dto.BrojPolise;
                p.DatumPocetka = dto.DatumPocetka; p.DatumIsteka = dto.DatumIsteka;
                p.Status = dto.Status; p.OsnovnaPremija = dto.OsnovnaPremija;
                p.Valuta = dto.Valuta; p.NacinPlacanja = dto.NacinPlacanja;
                p.Ugovarac = s.Load<Klijent>(dto.UgovaracId);
                p.Agent = dto.AgentId.HasValue ? s.Load<Agent>(dto.AgentId.Value) : null;
                s.SaveOrUpdate(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void obrisiPolisu(int id)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Polisa   p = s.Load<Polisa>(id);
                s.Delete(p);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static PolisaPregled mapPolisaPregled(Polisa p)
        {
            return new PolisaPregled
            {
                PolisaId = p.PolisaId, BrojPolise = p.BrojPolise,
                TipOsiguranja = p.TipOsiguranja,
                DatumPocetka = p.DatumPocetka, DatumIsteka = p.DatumIsteka,
                Status = p.Status, OsnovnaPremija = p.OsnovnaPremija,
                Valuta = p.Valuta, NacinPlacanja = p.NacinPlacanja,
                UgovaracId  = p.Ugovarac?.KlijentId ?? 0,
                UgovaracNaziv = p.Ugovarac?.Naziv,
                AgentId  = p.Agent?.OsobljeId,
                AgentIme = p.Agent != null ? $"{p.Agent.Ime} {p.Agent.Prezime}" : null
            };
        }
    }
}
