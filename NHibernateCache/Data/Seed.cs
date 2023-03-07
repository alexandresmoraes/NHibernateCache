namespace NHibernateCache.Data
{
  public class Seed
  {
    private readonly string[] _nomes = { "Maria", "Ana", "José", "Pedro", "Paulo", "Juliana", "Lucas", "Mateus", "Mariana", "Isabela" };
    private readonly string[] _sobrenomes = { "Silva", "Souza", "Costa", "Oliveira", "Pereira", "Ferreira", "Santos", "Rodrigues", "Alves", "Nascimento" };

    public IEnumerable<Pessoa> RandomPessoas()
    {
      return Enumerable.Range(1, 1000).Select(index =>
      new Pessoa
      {
        Nome = $"{_nomes[Random.Shared.Next(_nomes.Length)]} {_sobrenomes[Random.Shared.Next(_sobrenomes.Length)]}",
      })
      .ToArray();
    }
  }
}