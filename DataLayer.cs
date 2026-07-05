using System;
using System.Windows.Forms;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using OsiguranjApp.Mapiranja;

namespace OsiguranjApp
{
    public class DataLayer
    {
        private static ISessionFactory? _factory = null;
        private static readonly object _lock    = new object();

        public static ISession GetSession()
        {
            if (_factory == null)
            {
                lock (_lock)
                {
                    if (_factory == null)
                        _factory = CreateSessionFactory();
                }
            }
            return _factory.OpenSession();
        }

        private static ISessionFactory CreateSessionFactory()
        {
            try
            {
                // LOKALNO TESTIRANJE (Docker Oracle XE) - PRE PUSH-a NA FAKULTET vratiti na gislab konekciju!
                var cfg = OracleManagedDataClientConfiguration.Oracle10
                    .ConnectionString(c => c.Is(
                        "Data Source=localhost:1521/XEPDB1;" +
                        "User Id=S00000;Password=S00000;"));

                // ORIGINAL (fakultetski server) - IZMENITI: zameniti S00000 vasim brojem indeksa (npr. S11000)
                // var cfg = OracleManagedDataClientConfiguration.Oracle10
                //     .ConnectionString(c => c.Is(
                //         "Data Source=gislab-oracle.elfak.ni.ac.rs:1521/SBP_PDB;" +
                //         "User Id=S00000;Password=S00000;"));

                return Fluently.Configure()
                    .Database(cfg)
                    .Mappings(m => m.FluentMappings
                        .AddFromAssemblyOf<KlijentMapiranje>())
                    .BuildSessionFactory();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Greška pri povezivanju na bazu:\n\n" + ex.Message,
                    "Greška konekcije",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return null!;
            }
        }
    }
}
