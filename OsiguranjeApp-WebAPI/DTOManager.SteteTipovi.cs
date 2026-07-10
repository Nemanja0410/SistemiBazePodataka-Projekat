using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using OsiguranjApp.DTOs;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp
{
    // Kreiranje/citanje konkretnih podtipova stete (Steta je bazna tabela).
    public partial class DTOManager
    {
        private static Steta popuniBazuStete(Steta st, StetaPregled dto, ISession s)
        {
            st.BrojStete = dto.BrojStete;
            st.DatumPrijave = DateTime.Today;
            st.DatumNastanka = dto.DatumNastanka;
            st.Status = dto.Status ?? "PRIJAVLJENA";
            st.OpisDogodjaja = dto.OpisDogodjaja;
            st.Lokacija = dto.Lokacija;
            st.ProcenjeniIznos = dto.ProcenjeniIznos;
            st.Polisa = s.Load<Polisa>(dto.PolisaId);
            st.Podnosilac = s.Load<Klijent>(dto.PodnosilacId);
            return st;
        }

        // ---------- AutoSteta ----------

        public static List<AutoStetaPregled> vratiSveAutoStete()
        {
            var lista = new List<AutoStetaPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var a in s.Query<AutoSteta>().OrderByDescending(x => x.DatumPrijave))
                    lista.Add(mapAutoStetaPregled(a));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajAutoStetu(AutoStetaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var a = new AutoSteta
                {
                    VrstaStete = "AUTO",
                    ZapisnikPolicije = dto.ZapisnikPolicije,
                    Servis = dto.Servis,
                    Vozilo = dto.VoziloId.HasValue ? s.Load<Vozilo>(dto.VoziloId.Value) : null
                };
                popuniBazuStete(a, dto, s);
                s.Save(a);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajAutoStetu(AutoStetaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                AutoSteta a = s.Load<AutoSteta>(dto.StetaId);
                popuniBazuStete(a, dto, s);
                a.ZapisnikPolicije = dto.ZapisnikPolicije;
                a.Servis = dto.Servis;
                a.Vozilo = dto.VoziloId.HasValue ? s.Load<Vozilo>(dto.VoziloId.Value) : null;
                s.SaveOrUpdate(a);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static AutoStetaPregled mapAutoStetaPregled(AutoSteta a) => new AutoStetaPregled
        {
            StetaId = a.StetaId, BrojStete = a.BrojStete,
            DatumPrijave = a.DatumPrijave, DatumNastanka = a.DatumNastanka,
            PolisaId = a.Polisa?.PolisaId ?? 0, BrojPolise = a.Polisa?.BrojPolise,
            PodnosilacId = a.Podnosilac?.KlijentId ?? 0, PodnosilacNaziv = a.Podnosilac?.Naziv,
            VrstaStete = a.VrstaStete, OpisDogodjaja = a.OpisDogodjaja,
            Lokacija = a.Lokacija, Status = a.Status, ProcenjeniIznos = a.ProcenjeniIznos,
            ZapisnikPolicije = a.ZapisnikPolicije, Servis = a.Servis,
            VoziloId = a.Vozilo?.VoziloId, VoziloOpis = a.Vozilo?.ToString()
        };

        // ---------- ZdravstvenaSteta ----------

        public static List<ZdravstvenaStetaPregled> vratiSveZdravstveneStete()
        {
            var lista = new List<ZdravstvenaStetaPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var z in s.Query<ZdravstvenaSteta>().OrderByDescending(x => x.DatumPrijave))
                    lista.Add(mapZdravstvenaStetaPregled(z));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajZdravstvenuStetu(ZdravstvenaStetaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR");
            try
            {
                ISession s = DataLayer.GetSession();
                var z = new ZdravstvenaSteta
                {
                    VrstaStete = "ZDRAVSTVENA",
                    Dijagnoza = dto.Dijagnoza,
                    MedicinskaDocumentacija = dto.MedicinskaDocumentacija,
                    ZdravstvenaUstanova = dto.ZdravstvenaUstanova,
                    Lekar = dto.LekarId.HasValue ? s.Load<Lekar>(dto.LekarId.Value) : null
                };
                popuniBazuStete(z, dto, s);
                s.Save(z);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajZdravstvenuStetu(ZdravstvenaStetaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT", "LEKAR");
            try
            {
                ISession s = DataLayer.GetSession();
                ZdravstvenaSteta z = s.Load<ZdravstvenaSteta>(dto.StetaId);
                popuniBazuStete(z, dto, s);
                z.Dijagnoza = dto.Dijagnoza;
                z.MedicinskaDocumentacija = dto.MedicinskaDocumentacija;
                z.ZdravstvenaUstanova = dto.ZdravstvenaUstanova;
                z.Lekar = dto.LekarId.HasValue ? s.Load<Lekar>(dto.LekarId.Value) : null;
                s.SaveOrUpdate(z);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static ZdravstvenaStetaPregled mapZdravstvenaStetaPregled(ZdravstvenaSteta z) => new ZdravstvenaStetaPregled
        {
            StetaId = z.StetaId, BrojStete = z.BrojStete,
            DatumPrijave = z.DatumPrijave, DatumNastanka = z.DatumNastanka,
            PolisaId = z.Polisa?.PolisaId ?? 0, BrojPolise = z.Polisa?.BrojPolise,
            PodnosilacId = z.Podnosilac?.KlijentId ?? 0, PodnosilacNaziv = z.Podnosilac?.Naziv,
            VrstaStete = z.VrstaStete, OpisDogodjaja = z.OpisDogodjaja,
            Lokacija = z.Lokacija, Status = z.Status, ProcenjeniIznos = z.ProcenjeniIznos,
            Dijagnoza = z.Dijagnoza, MedicinskaDocumentacija = z.MedicinskaDocumentacija,
            ZdravstvenaUstanova = z.ZdravstvenaUstanova,
            LekarId = z.Lekar?.OsobljeId, LekarIme = z.Lekar != null ? $"{z.Lekar.Ime} {z.Lekar.Prezime}" : null
        };

        // ---------- ImovinskSteta ----------

        public static List<ImovinskStetaPregled> vratiSveImovinskeStete()
        {
            var lista = new List<ImovinskStetaPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                foreach (var i in s.Query<ImovinskSteta>().OrderByDescending(x => x.DatumPrijave))
                    lista.Add(mapImovinskStetaPregled(i));
                s.Close();
            }
            catch (Exception) { throw; }
            return lista;
        }

        public static void dodajImovinskuStetu(ImovinskStetaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                var i = new ImovinskSteta
                {
                    VrstaStete = "IMOVINSKA",
                    ProcenaOstecenja = dto.ProcenaOstecenja,
                    IzvodjacSanacije = dto.IzvodjacSanacije
                };
                popuniBazuStete(i, dto, s);
                s.Save(i);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        public static void azurirajImovinskuStetu(ImovinskStetaPregled dto)
        {
            ProveriOvlascenje("ADMIN", "AGENT");
            try
            {
                ISession s = DataLayer.GetSession();
                ImovinskSteta i = s.Load<ImovinskSteta>(dto.StetaId);
                popuniBazuStete(i, dto, s);
                i.ProcenaOstecenja = dto.ProcenaOstecenja;
                i.IzvodjacSanacije = dto.IzvodjacSanacije;
                s.SaveOrUpdate(i);
                s.Flush();
                s.Close();
            }
            catch (Exception) { throw; }
        }

        private static ImovinskStetaPregled mapImovinskStetaPregled(ImovinskSteta i) => new ImovinskStetaPregled
        {
            StetaId = i.StetaId, BrojStete = i.BrojStete,
            DatumPrijave = i.DatumPrijave, DatumNastanka = i.DatumNastanka,
            PolisaId = i.Polisa?.PolisaId ?? 0, BrojPolise = i.Polisa?.BrojPolise,
            PodnosilacId = i.Podnosilac?.KlijentId ?? 0, PodnosilacNaziv = i.Podnosilac?.Naziv,
            VrstaStete = i.VrstaStete, OpisDogodjaja = i.OpisDogodjaja,
            Lokacija = i.Lokacija, Status = i.Status, ProcenjeniIznos = i.ProcenjeniIznos,
            ProcenaOstecenja = i.ProcenaOstecenja, IzvodjacSanacije = i.IzvodjacSanacije
        };
    }
}
