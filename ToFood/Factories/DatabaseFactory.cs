using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.DB.NonRelational;
using ToFood.Domain.DB.Relational.PostgreSQL;
using System.Text;
using ToFood.Domain.DB.NonRelational.MongoDB;

namespace ToFood.Domain.Factories;

/// <summary>
/// Fábrica de configuração de bancos de dados (relacionais e não-relacionais).
/// </summary>
public static class DatabaseFactory
{
    /// <summary>
    /// Configura globalmente o console para aceitar UTF-8 (emojis e caracteres especiais)
    /// </summary>
    static DatabaseFactory()
    {
        Console.OutputEncoding = Encoding.UTF8;
    }

    /// <summary>
    /// Configura os bancos de dados no container de injeção de dependências.
    /// </summary>
    /// <param name="services">O contêiner de serviços.</param>
    /// <param name="configuration">As configurações do aplicativo.</param>
    public static void ConfigureDatabases(IServiceCollection services, IConfiguration configuration)
    {
        // Configura banco relacional
        ConfigureRelationalDatabase(services, configuration);

        // Configura banco não-relacional
        ConfigureNonRelationalDatabase(services, configuration);
    }

    /// <summary>
    /// Configura o banco relacional (PostgreSQL ou MySQL) com base no tipo especificado.
    /// </summary>
    private static void ConfigureRelationalDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var relationalDatabaseType = configuration["RelationalDatabaseType"];
        switch (relationalDatabaseType)
        {
            case "PostgreSQL":
                // Recupera a connection string do PostgreSQL
                var postgreSqlConnectionString = configuration.GetConnectionString("PostgreSQL") ?? "";
                var postgreSqlDatabaseName = relationalDatabaseType;

                // Configura o DbContext para PostgreSQL
                services.AddDbContext<ToFoodRelationalContext, PostgreSqlContext>(options =>
                    options.UseNpgsql(postgreSqlConnectionString)
                        .EnableDetailedErrors());

                // Testa a conexão com PostgreSQL
                TestDatabaseConnection(postgreSqlConnectionString, postgreSqlDatabaseName);
                break;

            /*
            case "MySQL":
                var mySqlConnectionString = configuration.GetConnectionString("MySQL");
                var mySqlDatabaseName = "MySQL";

                Console.WriteLine($"Usando connection string para {mySqlDatabaseName}: {mySqlConnectionString}");

                services.AddDbContext<ToFoodRelationalContext, MySqlContext>(options =>
                    options.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString)));

                TestDatabaseConnection(mySqlConnectionString, mySqlDatabaseName);
                break;
            */

            default:
                throw new InvalidOperationException("Tipo de banco relacional não suportado. Use 'PostgreSQL' ou 'MySQL'.");
        }
    }

    /// <summary>
    /// Configura o banco não-relacional (MongoDB).
    /// </summary>
    private static void ConfigureNonRelationalDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var nonRelationalDatabaseType = configuration["NonRelationalDatabaseType"];
        switch (nonRelationalDatabaseType)
        {
            case "MongoDB":
                // Recupera a string de conexão completa do MongoDB
                var mongoConnectionString = configuration.GetConnectionString("MongoDB") ?? "";

                // Extrai o nome do banco da string de conexão
                var mongoUrl = new MongoUrl(mongoConnectionString);
                var mongoDatabaseName = nonRelationalDatabaseType;

                // Registra o MongoLogContext como ToFoodNonRelationalContext
                services.AddSingleton<ToFoodNonRelationalContext>(_ =>
                    new MongoContext(mongoConnectionString, mongoDatabaseName));

                // Testa a conexão com o MongoDB
                TestDatabaseConnection(mongoConnectionString, mongoDatabaseName);
                break;

            default:
                throw new InvalidOperationException("Tipo de banco não-relacional não suportado. Use 'MongoDB'.");
        }
    }

    /// <summary>
    /// Realiza um teste de conexão com qualquer tipo de banco de dados.
    /// </summary>
    /// <param name="connectionString">A string de conexão do banco de dados.</param>
    /// <param name="databaseName">O nome do banco de dados (se aplicável).</param>
    private static void TestDatabaseConnection(string connectionString, string? databaseName = null)
    {
        try
        {
            switch (databaseName)
            {
                case "PostgreSQL":
                    using (var connection = new Npgsql.NpgsqlConnection(connectionString))
                    {
                        connection.Open(); // Tenta abrir a conexão
                        Console.WriteLine($"🐘 {databaseName} - Conexão bem sucedida com [Banco Relacional]. connectionString: [{connectionString}]");
                    }
                    break;

                case "MongoDB":
                    var mongoClient = new MongoClient(connectionString); // Cria o cliente MongoDB com a string de conexão.
                    var mongoUrl = new MongoUrl(connectionString);       // Analisa a string de conexão para extrair informações.
                    var mongoDatabaseName = mongoUrl.DatabaseName;       // Obtém o nome do banco da string de conexão.
                    var database = mongoClient.GetDatabase(mongoDatabaseName); // Obtém o banco de dados especificado.

                    // Testa se a conexão está funcional listando as coleções
                    database.ListCollectionNames();
                    Console.WriteLine($"🍃 {databaseName} - Conexão bem sucedida com [Banco Não Relacional]. connectionString: [{connectionString}]");
                    break;

                default:
                    throw new InvalidOperationException($"Tipo de banco de dados '{databaseName}' não suportado.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Falha ao conectar ao banco '{databaseName}': {ex.Message}");
        }
    }

}
