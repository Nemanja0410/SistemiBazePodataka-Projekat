using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using NHibernate;
using NHibernate.Linq;
using OsiguranjApp.DTOs;
using OsiguranjApp.Entiteti;

namespace OsiguranjApp
{
    public partial class DTOManager
    {
        private const int MaxNeuspesnihPrijava = 5;
        private const int PbkdfIteracije        = 100000;

        public static List<OsobljePregled> vratiOsobljeZaRegistraciju()
        {
            var lista = new List<OsobljePregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                var zauzeti = s.Query<Nalog>()
                    .Where(n => n.StatusNaloga == "NA_CEKANJU" || n.StatusNaloga == "ODOBREN")
                    .Where(n => n.Osoblje != null)
                    .Select(n => n.Osoblje!.OsobljeId)
                    .ToList();
                foreach (var o in s.Query<Osoblje>().Where(o => !zauzeti.Contains(o.OsobljeId)).OrderBy(o => o.Prezime))
                    lista.Add(mapOsobljePregled(o));
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static (bool uspeh, string poruka) registrujNalog(RegistracijaZahtev z)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(z.KorisnickoIme) || string.IsNullOrWhiteSpace(z.Lozinka))
                    return (false, "Korisničko ime i lozinka su obavezni.");
                if (z.Lozinka.Length < 8 || !z.Lozinka.Any(char.IsDigit))
                    return (false, "Lozinka mora imati bar 8 karaktera i bar jednu cifru.");

                ISession s = DataLayer.GetSession();

                if (s.Query<Nalog>().Any(n => n.KorisnickoIme == z.KorisnickoIme))
                {
                    s.Close();
                    return (false, "Korisničko ime je već zauzeto.");
                }

                Osoblje osoblje = s.Load<Osoblje>(z.OsobljeId);

                var postojeci = s.Query<Nalog>().Where(n => n.Osoblje != null && n.Osoblje.OsobljeId == z.OsobljeId).ToList();
                foreach (var stari in postojeci)
                {
                    if (stari.StatusNaloga == "NA_CEKANJU" || stari.StatusNaloga == "ODOBREN")
                    {
                        s.Close();
                        return (false, "Za ovog zaposlenog već postoji nalog na čekanju ili odobren nalog.");
                    }
                    if (stari.StatusNaloga == "ODBIJEN")
                        s.Delete(stari);
                }

                var (hash, salt) = HashLozinku(z.Lozinka);
                var n2 = new Nalog
                {
                    KorisnickoIme     = z.KorisnickoIme,
                    LozinkaHash       = hash,
                    LozinkaSalt       = salt,
                    LozinkaIteracije  = PbkdfIteracije,
                    Osoblje           = osoblje,
                    Uloga             = "NEODOBREN",
                    StatusNaloga      = "NA_CEKANJU",
                    DatumRegistracije = DateTime.Now
                };
                s.Save(n2);
                s.Flush();
                s.Close();
                return (true, "Zahtev za registraciju je poslat. Sačekajte odobrenje administratora.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška");
                return (false, "Greška prilikom registracije.");
            }
        }

        public static PrijavaRezultat prijaviSe(string korisnickoIme, string lozinka)
        {
            var rez = new PrijavaRezultat { Uspesno = false };
            try
            {
                ISession s = DataLayer.GetSession();
                Nalog? n = s.Query<Nalog>().FirstOrDefault(x => x.KorisnickoIme == korisnickoIme);

                if (n == null)
                {
                    ZabelezPokusaj(s, null, korisnickoIme, false, "NEPOSTOJECI_NALOG");
                    rez.Poruka = "Pogrešno korisničko ime ili lozinka.";
                    s.Close();
                    return rez;
                }

                if (n.StatusNaloga == "ZAKLJUCAN")
                {
                    ZabelezPokusaj(s, n, korisnickoIme, false, "NALOG_ZAKLJUCAN");
                    rez.Poruka = "Nalog je zaključan zbog previše neuspešnih prijava. Obratite se administratoru.";
                    s.Close();
                    return rez;
                }

                if (n.StatusNaloga == "NA_CEKANJU" || n.StatusNaloga == "ODBIJEN")
                {
                    ZabelezPokusaj(s, n, korisnickoIme, false, "NALOG_NIJE_ODOBREN");
                    rez.Poruka = n.StatusNaloga == "NA_CEKANJU"
                        ? "Nalog čeka odobrenje administratora."
                        : "Zahtev za nalog je odbijen.";
                    s.Close();
                    return rez;
                }

                bool ispravna = ProveriLozinku(lozinka, n.LozinkaHash!, n.LozinkaSalt!, n.LozinkaIteracije);
                if (!ispravna)
                {
                    n.NeuspesnihPrijava++;
                    if (n.NeuspesnihPrijava >= MaxNeuspesnihPrijava)
                        n.StatusNaloga = "ZAKLJUCAN";
                    s.Update(n);
                    s.Flush();
                    ZabelezPokusaj(s, n, korisnickoIme, false, "POGRESNA_LOZINKA");
                    rez.Poruka = n.StatusNaloga == "ZAKLJUCAN"
                        ? "Pogrešna lozinka. Nalog je sada zaključan zbog previše neuspešnih pokušaja."
                        : "Pogrešno korisničko ime ili lozinka.";
                    s.Close();
                    return rez;
                }

                n.NeuspesnihPrijava = 0;
                n.ZadnjaPrijava     = DateTime.Now;
                s.Update(n);
                s.Flush();
                ZabelezPokusaj(s, n, korisnickoIme, true, "USPESNO");

                rez.Uspesno = true;
                rez.Poruka  = "Uspešna prijava.";
                rez.Nalog   = mapNalogPregled(n);
                s.Close();
                return rez;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška");
                rez.Poruka = "Greška prilikom prijave.";
                return rez;
            }
        }

        public static List<NalogPregled> vratiNeodobreneNaloge()
        {
            ProveriOvlascenje("ADMIN");
            return vratiNalogeSaStatusom("NA_CEKANJU");
        }

        public static List<NalogPregled> vratiSveNaloge()
        {
            ProveriOvlascenje("ADMIN");
            return vratiNalogeSaStatusom(null);
        }

        private static List<NalogPregled> vratiNalogeSaStatusom(string? status)
        {
            var lista = new List<NalogPregled>();
            try
            {
                ISession s = DataLayer.GetSession();
                var q = s.Query<Nalog>().AsQueryable();
                if (!string.IsNullOrEmpty(status))
                    q = q.Where(n => n.StatusNaloga == status);
                var sortirano = q.ToList()
                    .OrderByDescending(n => n.Uloga == "ADMIN")
                    .ThenByDescending(n => n.DatumRegistracije);
                foreach (var n in sortirano)
                    lista.Add(mapNalogPregled(n));
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
            return lista;
        }

        public static void odobriNalog(int nalogId, string dodeljenaUloga)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Nalog n = s.Load<Nalog>(nalogId);
                if (n.Uloga == "ADMIN")
                {
                    MessageBox.Show("Status admin naloga se ne može menjati iz ovog ekrana.", "Greška");
                    s.Close();
                    return;
                }
                if (n.StatusNaloga != "NA_CEKANJU")
                {
                    MessageBox.Show("Samo nalozi na čekanju mogu biti odobreni.", "Greška");
                    s.Close();
                    return;
                }
                if (dodeljenaUloga != "ADMIN" && n.Osoblje != null && n.Osoblje.TipOsoblja != dodeljenaUloga)
                {
                    MessageBox.Show("Dodeljena uloga se mora poklapati sa tipom zaposlenog.", "Greška");
                    s.Close();
                    return;
                }
                n.Uloga          = dodeljenaUloga;
                n.StatusNaloga   = "ODOBREN";
                n.DatumOdobrenja = DateTime.Now;
                n.OdobrioNalog   = s.Load<Nalog>(SesijaKorisnik.TrenutniNalog!.NalogId);
                s.Update(n);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void odbijNalog(int nalogId)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Nalog n = s.Load<Nalog>(nalogId);
                if (n.Uloga == "ADMIN")
                {
                    MessageBox.Show("Admin nalozi ne mogu biti odbijeni.", "Greška");
                    s.Close();
                    return;
                }
                if (n.StatusNaloga != "NA_CEKANJU")
                {
                    MessageBox.Show("Samo nalozi na čekanju mogu biti odbijeni.", "Greška");
                    s.Close();
                    return;
                }
                n.StatusNaloga = "ODBIJEN";
                s.Update(n);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void zakljucajNalog(int nalogId)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Nalog n = s.Load<Nalog>(nalogId);
                if (n.Uloga == "ADMIN")
                {
                    MessageBox.Show("Admin nalozi ne mogu biti zaključani.", "Greška");
                    s.Close();
                    return;
                }
                if (n.StatusNaloga != "ODOBREN")
                {
                    MessageBox.Show("Samo odobreni nalozi mogu biti zaključani.", "Greška");
                    s.Close();
                    return;
                }
                n.StatusNaloga = "ZAKLJUCAN";
                s.Update(n);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void otkljucajNalog(int nalogId)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Nalog n = s.Load<Nalog>(nalogId);
                if (n.Uloga == "ADMIN")
                {
                    MessageBox.Show("Admin nalozi ne mogu biti otključani iz ovog ekrana.", "Greška");
                    s.Close();
                    return;
                }
                if (n.StatusNaloga != "ZAKLJUCAN")
                {
                    MessageBox.Show("Samo zaključani nalozi mogu biti otključani.", "Greška");
                    s.Close();
                    return;
                }
                n.StatusNaloga      = "ODOBREN";
                n.NeuspesnihPrijava = 0;
                s.Update(n);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static void obrisiNalog(int nalogId)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Nalog n = s.Load<Nalog>(nalogId);
                if (n.Uloga == "ADMIN")
                {
                    MessageBox.Show("Admin nalozi ne mogu biti obrisani iz ovog ekrana.", "Greška");
                    s.Close();
                    return;
                }
                s.Delete(n);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        public static (bool uspeh, string poruka) promeniLozinku(int nalogId, string staraLozinka, string novaLozinka)
        {
            try
            {
                bool jeAdminZaTudjiNalog = SesijaKorisnik.ImaUlogu("ADMIN") && SesijaKorisnik.TrenutniNalog!.NalogId != nalogId;
                if (!jeAdminZaTudjiNalog)
                    ProveriOvlascenje("ADMIN", "AGENT", "LEKAR", "PRAVNIK", "PROCENITELJ");

                if (novaLozinka.Length < 8 || !novaLozinka.Any(char.IsDigit))
                    return (false, "Nova lozinka mora imati bar 8 karaktera i bar jednu cifru.");

                ISession s = DataLayer.GetSession();
                Nalog n = s.Load<Nalog>(nalogId);

                if (!jeAdminZaTudjiNalog)
                {
                    bool ispravna = ProveriLozinku(staraLozinka, n.LozinkaHash!, n.LozinkaSalt!, n.LozinkaIteracije);
                    if (!ispravna)
                    {
                        s.Close();
                        return (false, "Stara lozinka nije ispravna.");
                    }
                }

                var (hash, salt) = HashLozinku(novaLozinka);
                n.LozinkaHash          = hash;
                n.LozinkaSalt          = salt;
                n.LozinkaIteracije     = PbkdfIteracije;
                n.MoraPromenitiLozinku = false;
                s.Update(n);
                s.Flush();
                s.Close();
                return (true, "Lozinka je uspešno promenjena.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška");
                return (false, "Greška prilikom promene lozinke.");
            }
        }

        public static void resetujLozinku(int nalogId, string privremenaLozinka)
        {
            ProveriOvlascenje("ADMIN");
            try
            {
                ISession s = DataLayer.GetSession();
                Nalog n = s.Load<Nalog>(nalogId);
                var (hash, salt) = HashLozinku(privremenaLozinka);
                n.LozinkaHash          = hash;
                n.LozinkaSalt          = salt;
                n.LozinkaIteracije     = PbkdfIteracije;
                n.MoraPromenitiLozinku = true;
                n.NeuspesnihPrijava    = 0;
                if (n.StatusNaloga == "ZAKLJUCAN") n.StatusNaloga = "ODOBREN";
                s.Update(n);
                s.Flush();
                s.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Greška"); }
        }

        private static void ZabelezPokusaj(ISession s, Nalog? n, string korisnickoIme, bool uspesno, string razlog)
        {
            var zapis = new IstorijaPrijave
            {
                Nalog                = n,
                KorisnickoImePokusaj = korisnickoIme,
                VremePokusaja        = DateTime.Now,
                Uspesno              = uspesno,
                Razlog               = razlog
            };
            s.Save(zapis);
            s.Flush();
        }

        private static NalogPregled mapNalogPregled(Nalog n) => new NalogPregled
        {
            NalogId              = n.NalogId,
            KorisnickoIme        = n.KorisnickoIme,
            OsobljeId            = n.Osoblje?.OsobljeId,
            ImeOsoblja           = n.Osoblje?.Ime,
            PrezimeOsoblja       = n.Osoblje?.Prezime,
            TipOsoblja           = n.Osoblje?.TipOsoblja,
            Uloga                = n.Uloga,
            StatusNaloga         = n.StatusNaloga,
            MoraPromenitiLozinku = n.MoraPromenitiLozinku,
            NeuspesnihPrijava    = n.NeuspesnihPrijava,
            DatumRegistracije    = n.DatumRegistracije,
            DatumOdobrenja       = n.DatumOdobrenja,
            ZadnjaPrijava        = n.ZadnjaPrijava
        };

        private static (string hash, string salt) HashLozinku(string lozinka)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            using var pbkdf2 = new Rfc2898DeriveBytes(lozinka, saltBytes, PbkdfIteracije, HashAlgorithmName.SHA256);
            byte[] hashBytes = pbkdf2.GetBytes(32);
            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        private static bool ProveriLozinku(string lozinka, string hash, string salt, int iteracije)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(lozinka, saltBytes, iteracije, HashAlgorithmName.SHA256);
            byte[] hashBytes = pbkdf2.GetBytes(32);
            byte[] ocekivano = Convert.FromBase64String(hash);
            return CryptographicOperations.FixedTimeEquals(hashBytes, ocekivano);
        }
    }
}
