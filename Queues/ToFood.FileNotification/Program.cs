using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ToFood.Domain.Extensions;
using ToFood.Domain.Factories;
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
                // Configuração do banco de dados usando o DatabaseFactory
                DatabaseFactory.ConfigureDatabases(services, context.Configuration);

                // Configuração de serviços do domínio
                services.AddDomainServices();

                // Registra o Worker como um HostedService
                services.AddHostedService<SqsNotificationWorker>();

                // Configuração do logging
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });
            })
            .Build();

        await builder.RunAsync();
    }
}
