using Microsoft.AspNetCore.Mvc;
using NHibernateCache.Data;

namespace NHibernateCache.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class PessoaController : ControllerBase
  {
    private readonly string[] _nomes = { "Maria", "Ana", "José", "Pedro", "Paulo", "Juliana", "Lucas", "Mateus", "Mariana", "Isabela" };
    private readonly string[] _sobrenomes = { "Silva", "Souza", "Costa", "Oliveira", "Pereira", "Ferreira", "Santos", "Rodrigues", "Alves", "Nascimento" };

    private readonly ILogger<PessoaController> _logger;
    private readonly SessionManager _sessionManager;

    public PessoaController(ILogger<PessoaController> logger, SessionManager sessionManager)
    {
      _logger = logger;
      _sessionManager = sessionManager;
    }

    [HttpGet("seed")]
    public IEnumerable<Pessoa> Seed()
    {
      var seed = new Seed();
      var pessoas = seed.RandomPessoas();

      _sessionManager.UsingSession((session) =>
      {
        foreach (var pessoa in pessoas)
        {
          session.Save(pessoa);
        }
      });

      return pessoas;
    }

    [HttpGet("/All")]
    public IEnumerable<Pessoa> GetPessoas([FromQuery] int page, [FromQuery] int limit)
    {
      IEnumerable<Pessoa>? pessoas = null;

      _sessionManager.UsingSession((session) =>
      {
        pessoas = session.QueryOver<Pessoa>()
        .Skip(page * limit).Take(limit).Cacheable()
        .List();
      });

      return pessoas!;
    }

    [HttpGet("{id}")]
    public Pessoa GetPessoaById([FromRoute] int id)
    {
      Pessoa? pessoa = null;

      _sessionManager.UsingSession((session) =>
      {
        pessoa = session.Get<Pessoa>(id);
      });

      return pessoa;
    }
  }
}