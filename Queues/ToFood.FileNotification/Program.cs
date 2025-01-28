using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ToFood.Domain.Entities.NonRelational;
using ToFood.Domain.Extensions;
using ToFood.Domain.Factories;
using ToFood.Domain.Helpers;
using ToFood.Queues.FileNotification;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configBuilder =>
            {
                // Configuração do caminho para o appsettings.json
                configBuilder.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\"))
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Registra o IHttpContextAccessor
                services.AddHttpContextAccessor();

                // Configuração do banco de dados usando o DatabaseFactory
                DatabaseFactory.ConfigureDatabases(services, configuration);

                // Registra o Worker como um HostedService
                services.AddHostedService<SqsNotificationWorker>();

                // Configuração de logging
                services.AddLogging(loggingBuilder =>
                {


                    // Recupera o ServiceProvider para resolver dependências
                    var serviceProvider = services.BuildServiceProvider();
                    var logCollection = serviceProvider.GetRequiredService<IMongoCollection<Log>>();
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddConsole();
                    // Configura o MongoDBLoggerProvider
                    loggingBuilder.AddProvider(new MongoDBLoggerProvider(logCollection, httpContextAccessor));

                    // DI (Injeção de Dependência)
                    // Registra os serviços do domínio
                    services.AddDomainServices();
                });

            })
            .Build();

        await builder.RunAsync();
    }
}