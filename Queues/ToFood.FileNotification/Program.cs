using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ToFood.Domain.Extensions;
using ToFood.Queues.FileNotification;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddDomainServices();
                services.AddHostedService<SqsNotificationWorker>();
                services.AddLogging(configure => configure.AddConsole());
            })
            .Build();

        await host.RunAsync();
    }
}
