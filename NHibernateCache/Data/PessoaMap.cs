using FluentNHibernate.Mapping;

namespace NHibernateCache.Data
{
  public sealed class PessoaMap : ClassMap<Pessoa>
  {
    public PessoaMap()
    {
      Table("pessoa");

      Cache.ReadWrite();

      Id(x => x.Id).GeneratedBy.Identity();
      Map(x => x.Nome);
    }
  }
}