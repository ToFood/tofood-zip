﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.DB.NonRelational;
using ToFood.Domain.DB.Relational.PostgreSQL;

namespace ToFood.Domain.Factories;

/// <summary>
/// Fábrica de configuração de bancos de dados (relacionais e não-relacionais).
/// </summary>
public static class DatabaseFactory
{
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
                var postgreSqlDatabaseName = "PostgreSQL";

                // Exibe a connection string no console
                Console.WriteLine($"Usando connection string para {postgreSqlDatabaseName}: {postgreSqlConnectionString}");

                // Configura o DbContext para PostgreSQL
                services.AddDbContext<ToFoodRelationalContext, PostgreSqlContext>(options =>
                    options.UseNpgsql(postgreSqlConnectionString)
                        .EnableSensitiveDataLogging()
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
        var nonRelationalDatabaseType = "MongoDB"; // Pode ser parametrizado se suportar outros bancos no futuro
        switch (nonRelationalDatabaseType)
        {
            case "MongoDB":
                // Recupera a connection string e o nome do banco do MongoDB
                var mongoConnectionString = configuration.GetConnectionString("MongoDB:ConnectionString") ?? "";
                var mongoDatabaseName = configuration["ConnectionStrings:MongoDB:DatabaseName"] ?? "";

                // Exibe a connection string no console
                Console.WriteLine($"Usando connection string para {nonRelationalDatabaseType}: {mongoConnectionString}");

                // Configura o contexto do MongoDB
                services.AddSingleton<ToFoodNonRelationalContext>(_ =>
                    new ToFoodNonRelationalContext(mongoConnectionString, mongoDatabaseName));

                // Testa a conexão com o MongoDB
                TestMongoDatabaseConnection(mongoConnectionString, mongoDatabaseName);
                break;

            default:
                throw new InvalidOperationException("Tipo de banco não-relacional não suportado. Use 'MongoDB'.");
        }
    }

    /// <summary>
    /// Realiza um teste de conexão com um banco relacional.
    /// </summary>
    /// <param name="connectionString">A string de conexão do banco de dados.</param>
    /// <param name="databaseName">O nome do banco de dados (PostgreSQL, MySQL, etc.).</param>
    private static void TestDatabaseConnection(string connectionString, string databaseName)
    {
        try
        {
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            connection.Open(); // Tenta abrir a conexão
            Console.WriteLine($"Conexão com o banco relacional '{databaseName}' bem-sucedida.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Falha ao conectar no banco relacional '{databaseName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Realiza um teste de conexão com o MongoDB.
    /// </summary>
    /// <param name="connectionString">A string de conexão do MongoDB.</param>
    /// <param name="databaseName">O nome do banco de dados MongoDB.</param>
    private static void TestMongoDatabaseConnection(string connectionString, string databaseName)
    {
        try
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName); // Verifica se o banco é acessível
            database.ListCollectionNames(); // Tenta listar coleções como teste de conectividade
            Console.WriteLine($"Conexão com o banco não-relacional '{databaseName}' bem-sucedida.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Falha ao conectar ao banco não-relacional '{databaseName}': {ex.Message}");
        }
    }
}
