using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ToFood.Domain.Interfaces;

namespace ToFood.Queues.FileNotification;


public class SqsNotificationWorker : BackgroundService
{
    private readonly ILogger<SqsNotificationWorker> _logger;
    private readonly AmazonSQSClient _sqsClient;
    private readonly string _queueUrl;
    private readonly INotificationService _notificationService;

    public SqsNotificationWorker(
        ILogger<SqsNotificationWorker> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _sqsClient = new AmazonSQSClient();
        _queueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/MyQueue"; // Substituir pela URL real.
        _notificationService = notificationService;
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
                    MaxNumberOfMessages = 5, // Número máximo de mensagens para consumir em uma chamada.
                    WaitTimeSeconds = 10, // Long polling para reduzir custos.
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
            // Deserializar o corpo da mensagem para obter o ID da notificação.
            var notification = JsonSerializer.Deserialize<NotificationMessage>(message.Body);
            if (notification?.NotificationId == null)
            {
                throw new InvalidOperationException("Mensagem mal formatada: ID da notificação não encontrado.");
            }

            // Chamar o método de domínio para processar a notificação.
            await _notificationService.SendEmail(notification.NotificationId.Value);

            // Apagar a mensagem da fila após o processamento bem-sucedido.
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
        }
    }

    private class NotificationMessage
    {
        public long? NotificationId { get; set; }
    }
}
