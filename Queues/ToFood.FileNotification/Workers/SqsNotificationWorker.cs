using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ToFood.Domain.Interfaces;
using ToFood.Domain.Services.TokenManager;

namespace ToFood.Queues.FileNotification;

public class SqsNotificationWorker : BackgroundService
{
    private readonly ILogger<SqsNotificationWorker> _logger;
    private readonly INotificationService _notificationService;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly AmazonSQSClient _sqsClient;
    private readonly string _queueUrl;

    public SqsNotificationWorker(
        ILogger<SqsNotificationWorker> logger,
        INotificationService notificationService,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _logger = logger;
        _notificationService = notificationService;
        _configuration = configuration;

        _queueUrl = _configuration["AWS:SQSQueueUrl"] ?? throw new ArgumentNullException("SQSQueueUrl não configurado.");

        // Inicializa o cliente SQS uma vez para reutilização
        _sqsClient = AWSTokenManager.GetAWSClient(_configuration);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SQS Notification Worker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 5,
                    WaitTimeSeconds = 10
                };

                var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                foreach (var message in response.Messages)
                {
                    await ProcessMessageAsync(message, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consumir mensagens da fila SQS.");
            }
        }

        _logger.LogInformation("SQS Notification Worker finalizado.");
    }

    private async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
    {
        try
        {
            // Deserializar o corpo da mensagem para obter o ID da notificação
            var notification = JsonSerializer.Deserialize<NotificationMessage>(message.Body);
            if (notification?.NotificationId == null)
            {
                throw new InvalidOperationException("Mensagem mal formatada: ID da notificação não encontrado.");
            }

            // Processar a notificação
            await _notificationService.SendEmail(notification.NotificationId.Value);

            // Apagar a mensagem da fila após o processamento
            await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
            {
                QueueUrl = _queueUrl,
                ReceiptHandle = message.ReceiptHandle
            }, cancellationToken);

            _logger.LogInformation($"Mensagem processada com sucesso: ID={notification.NotificationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao processar mensagem: {message.Body}");

            // Opcional: Implementar lógica para mensagens mal formatadas
            // Ex.: Mover para uma Dead Letter Queue (DLQ)
        }
    }

    private class NotificationMessage
    {
        public long? NotificationId { get; set; }
    }
}
