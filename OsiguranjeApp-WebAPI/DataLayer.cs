using System;
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
                var cfg = OracleManagedDataClientConfiguration.Oracle10
                    .ConnectionString(c => c.Is(
                         "Data Source=gislab-oracle.elfak.ni.ac.rs:1521/SBP_PDB;" +
                         "User Id=S19451;Password=S19451;"));

                return Fluently.Configure()
                    .Database(cfg)
                    .Mappings(m => m.FluentMappings
                        .AddFromAssemblyOf<KlijentMapiranje>())
                    .BuildSessionFactory();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Greška pri povezivanju na bazu: " + ex.Message, ex);
            }
        }
    }
}
