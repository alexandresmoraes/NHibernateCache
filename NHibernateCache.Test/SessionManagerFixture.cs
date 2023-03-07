using NHibernateCache.Data;

namespace NHibernateCache.Test
{
  public class SessionManagerFixture : SessionManager
  {
    public SessionManagerFixture() : base(
      "<connectionStringPostgres>",
      "<connectionStringRedis>")
    {
    }

    [SetUp]
    public void Setup()
    {
      var seed = new Seed();

      UsingSession(session =>
      {
        foreach (var pessoa in seed.RandomPessoas())
        {
          session.Save(pessoa);
        }
      });
    }

    [Test]
    public void Should_Cache_Entity()
    {
      _sessionFactory!.Statistics.Clear();

      object? personId = null;

      UsingSession(session =>
      {
        personId = session.Save(new Pessoa
        {
          Nome = "Aurora Maria"
        });
      });

      _sessionFactory!.Statistics.Clear();

      UsingSession(session =>
      {
        session.Get<Pessoa>(personId);
        Assert.That(_sessionFactory.Statistics.SecondLevelCacheMissCount, Is.EqualTo(1));
        Assert.That(_sessionFactory.Statistics.SecondLevelCachePutCount, Is.EqualTo(1));
      });

      _sessionFactory.Statistics.Clear();

      UsingSession(session =>
      {
        session.Get<Pessoa>(personId);
        Assert.That(_sessionFactory.Statistics.SecondLevelCacheHitCount, Is.EqualTo(1));
        Assert.That(_sessionFactory.Statistics.SecondLevelCacheMissCount, Is.EqualTo(0));
        Assert.That(_sessionFactory.Statistics.SecondLevelCachePutCount, Is.EqualTo(0));
      });
    }

    [Test]
    public void Should_Cache_Query()
    {
      _sessionFactory!.Statistics.Clear();

      UsingSession(session =>
      {
        var pessoas = session.QueryOver<Pessoa>()
        .Skip(0).Take(25).Cacheable()
        .List();

        Assert.That(_sessionFactory.Statistics.SecondLevelCacheMissCount, Is.EqualTo(0));
        Assert.That(_sessionFactory.Statistics.SecondLevelCachePutCount, Is.EqualTo(25));
      });

      _sessionFactory.Statistics.Clear();

      UsingSession(session =>
      {
        var pessoas = session.QueryOver<Pessoa>()
        .Skip(0).Take(25).Cacheable()
        .List();

        Assert.That(_sessionFactory.Statistics.SecondLevelCacheHitCount, Is.EqualTo(1));
        Assert.That(_sessionFactory.Statistics.SecondLevelCacheMissCount, Is.EqualTo(0));
        Assert.That(_sessionFactory.Statistics.SecondLevelCachePutCount, Is.EqualTo(0));
      });

      _sessionFactory!.Statistics.Clear();
    }
  }
}