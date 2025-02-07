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
        string appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\", "appsettings.json");

        // Verifica se o arquivo appsettings.json existe
        if (!File.Exists(appSettingsPath))
        {
            // Cria o arquivo appsettings.json com valores padrão
            var defaultConfig = $@"
            {{
                ""AWS"": {{
                    ""SecretManager"": ""{Environment.GetEnvironmentVariable("AWS__SecretManager")}"",
                    ""AccessKey"": ""{Environment.GetEnvironmentVariable("AWS__AccessKey")}"",
                    ""SecretKey"": ""{Environment.GetEnvironmentVariable("AWS__SecretKey")}"",
                    ""Region"": ""{Environment.GetEnvironmentVariable("AWS__Region")}""
                }}
            }}";
            File.WriteAllText(appSettingsPath, defaultConfig);
            Console.WriteLine($"Arquivo appsettings.json criado em: {appSettingsPath}");
        }

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\"))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var config = configBuilder.Build();

        // Recuperar os segredos do AWS Secrets Manager
        var secrets = await SecretsHelper.GetSecretsAWS(config);

        // Adicionar os segredos ao IConfiguration
        foreach (var secret in secrets)
        {
            config[secret.Key] = secret.Value;
        }

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.AddConfiguration(config);
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