using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Helpers;

namespace ToFood.Tests;

/// <summary>
/// Classe base para configurar testes com banco de dados real.
/// </summary>
public abstract class TestOperationalBase : IDisposable
{
    /// <summary>
    /// Contexto do banco de dados relacional real para os testes operacionais.
    /// </summary>
    protected ToFoodRelationalContext RelationalContext { get; }

    /// <summary>
    /// Configuração do ambiente (IConfiguration com dados reais).
    /// </summary>
    protected IConfiguration Configuration { get; }

    /// <summary>
    /// Construtor que inicializa o contexto do banco de dados e a configuração do ambiente.
    /// </summary>
    protected TestOperationalBase()
    {
        // Carregar as configurações do arquivo appsettings.json
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\")) // Caminho manual para 3 níveis acima
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Recuperar os segredos do AWS Secrets Manager
        var secrets = SecretsHelper.GetSecretsAWS(Configuration).Result;

        // Adicionar os segredos ao IConfiguration
        foreach (var secret in secrets)
        {
            Configuration[secret.Key] = secret.Value;
        }


        // Obter a connection string do PostgreSQL
        var postgreSqlConnectionString = Configuration.GetConnectionString("PostgreSQL");

        if (string.IsNullOrWhiteSpace(postgreSqlConnectionString))
        {
            throw new InvalidOperationException("A connection string do PostgreSQL não foi encontrada no arquivo de configuração.");
        }

        // Configurar o DbContext para o banco real
        var options = new DbContextOptionsBuilder<ToFoodRelationalContext>()
            .UseNpgsql(postgreSqlConnectionString)
            .EnableDetailedErrors()
            .Options;

        RelationalContext = new ToFoodRelationalContext(options);
    }

    /// <summary>
    /// Método para limpar o banco de dados após os testes, se necessário.
    /// </summary>
    protected virtual void CleanupDatabase()
    {
        // Implementar limpeza de dados após o teste, se necessário
        // Por exemplo: Deletar ou resetar dados de teste
    }

    /// <summary>
    /// Libera recursos utilizados durante os testes.
    /// </summary>
    public void Dispose()
    {
        CleanupDatabase();
        RelationalContext.Dispose();
    }
}
