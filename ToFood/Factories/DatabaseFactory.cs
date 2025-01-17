using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        ConfigureRelationalDatabase(services, configuration);    // Configura banco relacional
        ConfigureNonRelationalDatabase(services, configuration); // Configura banco não-relacional
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
                services.AddDbContext<ToFoodRelationalContext, PostgreSqlContext>(options =>
                    options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));
                break;

            /*
            case "MySQL":
            services.AddDbContext<ToFoodRelationalContext, MySqlContext>(options =>
                options.UseMySql(configuration.GetConnectionString("MySQL"), ServerVersion.AutoDetect(configuration.GetConnectionString("MySQL"))));
            break;
            */

            default:
                throw new InvalidOperationException("Tipo de banco relacional não suportado. Use 'PostgreSQL'.");
        }
    }


    /// <summary>
    /// Configura o banco não-relacional (MongoDB).
    /// </summary>
    private static void ConfigureNonRelationalDatabase(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ToFoodNonRelationalContext>(_ =>
            new ToFoodNonRelationalContext(
                configuration["MongoDB:ConnectionString"] ?? "",
                configuration["MongoDB:DatabaseName"] ?? ""
            ));
    }

}
