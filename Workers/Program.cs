using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ToFood.Domain.Services;
using ToFood.VideoProcessor.Worker;

namespace ToFood.VideoProcessor;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Inicializando VideoProcessingWorker...");

        // Configura o Host Genérico
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Registra o serviço ZipService da camada Domain
                services.AddScoped<ZipService>();

                // Registra o VideoProcessingWorker como um Hosted Service (BackgroundService)
                services.AddHostedService<VideoProcessingWorker>();
            })
            .Build()
            .RunAsync();
    }
}
