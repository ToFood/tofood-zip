using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ToFood.Domain.Services;

namespace ToFood.VideoProcessor.Worker
{
    public class VideoProcessingWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VideoProcessingWorker> _logger;
        private readonly AmazonSQSClient _sqsClient;
        private readonly string _sqsQueueUrl;

        public VideoProcessingWorker(IServiceProvider serviceProvider, ILogger<VideoProcessingWorker> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _sqsClient = new AmazonSQSClient();
            _sqsQueueUrl = configuration["AWS:SQSQueueUrl"]; // Obtém o valor da configuração
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VideoProcessingWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var receiveMessageRequest = new ReceiveMessageRequest
                    {
                        QueueUrl = _sqsQueueUrl, // Usa a URL do SQS obtida das configurações
                        MaxNumberOfMessages = 5,
                        WaitTimeSeconds = 10
                    };

                    var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                    foreach (var message in response.Messages)
                    {
                        _logger.LogInformation($"Mensagem recebida: {message.Body}");

                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var zipService = scope.ServiceProvider.GetRequiredService<ZipService>();

                            var videoData = JsonSerializer.Deserialize<VideoMessage>(message.Body);

                            //await zipService.ConvertVideoToImagesAndZip(videoData.VideoId, videoData.FilePath);

                            await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                            {
                                QueueUrl = _sqsQueueUrl,
                                ReceiptHandle = message.ReceiptHandle
                            });

                            _logger.LogInformation($"Vídeo processado: {videoData.VideoId}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao processar mensagem.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao consumir mensagens do SQS.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    public class VideoMessage
    {
        public string VideoId { get; set; }
        public string FilePath { get; set; }
    }
}
