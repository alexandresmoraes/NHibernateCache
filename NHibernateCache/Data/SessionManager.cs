using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Caches.StackExchangeRedis;
using NHibernate.Caches.Util.JsonSerializer;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System.Diagnostics;
using Environment = NHibernate.Cfg.Environment;
using ISession = NHibernate.ISession;

namespace NHibernateCache.Data
{
  public class SessionManager : IDisposable
  {
    private static Configuration? _configuration;
    protected static ISessionFactory? _sessionFactory;

    public SessionManager(string connectionStringPostgres, string connectionStringRedis)
    {
      if (_configuration == null)
      {
        RedisCacheProvider.DefaultCacheConfiguration.Serializer = new JsonCacheSerializer();
        RedisCacheProvider.DefaultCacheConfiguration.DefaultExpiration = TimeSpan.FromMinutes(1);

        _configuration = Fluently.Configure()
          .Database(
            PostgreSQLConfiguration.PostgreSQL82
            .ConnectionString(connectionStringPostgres)
            .ShowSql()
            .FormatSql()
            .AdoNetBatchSize(0)
          )
          .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(true, true))
          .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Pessoa>())
          .Cache(
            c => c.ProviderClass<RedisCacheProvider>()
              .UseSecondLevelCache()
              .UseQueryCache()
              .UseMinimalPuts()
          )
          .ExposeConfiguration(cfg =>
          {
            cfg.Properties.Add("cache.configuration", connectionStringRedis);
            cfg.Properties.Add(Environment.GenerateStatistics, "true");
          })
          .BuildConfiguration();

        _sessionFactory = _configuration.BuildSessionFactory();
      }
    }

    public void UsingSession(Action<ISession> action)
    {
      using var session = _sessionFactory!.OpenSession();
      using var transaction = session.BeginTransaction();

      var timer = new Stopwatch();
      timer.Start();

      action(session);

      timer.Stop();

      transaction.Commit();

      Console.WriteLine($"Número de consultas: {session.SessionFactory.Statistics.QueryExecutionCount}");
      Console.WriteLine($"Número de hits no cache de segundo nível: {session.SessionFactory.Statistics.SecondLevelCacheHitCount}");
      Console.WriteLine($"Número de misses no cache de segundo nível: {session.SessionFactory.Statistics.SecondLevelCacheMissCount}");
      Console.WriteLine($"Número de put no cache de segundo nível: {session.SessionFactory.Statistics.SecondLevelCachePutCount}");
      Console.WriteLine($"Tempo da query: {timer.Elapsed}");
      Console.WriteLine();

      session.Clear();
    }

    public void Dispose()
    {
      _sessionFactory?.Dispose();
    }
  }
}