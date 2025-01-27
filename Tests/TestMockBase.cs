using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ToFood.Domain.DB.Relational;

namespace ToFood.Tests;

/// <summary>
/// Classe base para configurar o ambiente de testes.
/// </summary>
public abstract class TestMockBase : IDisposable
{
    /// <summary>
    /// Contexto do banco de dados relacional em memória para os testes.
    /// </summary>
    protected ToFoodRelationalContext RelationalContext { get; }

    /// <summary>
    /// Configuração do ambiente (mock do IConfiguration).
    /// </summary>
    protected IConfiguration Configuration { get; }

    /// <summary>
    /// Construtor que inicializa o contexto do banco de dados e a configuração do ambiente.
    /// </summary>
    protected TestMockBase()
    {
        // Configurar o DbContext InMemory
        var options = new DbContextOptionsBuilder<ToFoodRelationalContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        RelationalContext = new ToFoodRelationalContext(options);

        // Configurar o mock do IConfiguration
        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("Jwt:Key", "tofood!aA1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V2W3X4Y5Z6!"),
                new KeyValuePair<string, string?>("Jwt:Issuer", "your-issuer"),
                new KeyValuePair<string, string?>("Jwt:Audience", "your-audience")
            ])
            .Build();

        // Popular o banco com dados iniciais (opcional)
        SeedDatabase();
    }

    /// <summary>
    /// Método para popular o banco de dados com dados iniciais.
    /// Pode ser sobrescrito por classes derivadas.
    /// </summary>
    protected virtual void SeedDatabase()
    {

    }

    /// <summary>
    /// Libera recursos utilizados durante os testes.
    /// </summary>
    public void Dispose()
    {
        RelationalContext.Dispose();
    }
}
