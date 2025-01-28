using Amazon.SQS.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Text.Json;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Enums;
using ToFood.Domain.Extensions;
using ToFood.Domain.Interfaces;
using ToFood.Domain.Services.Notifications;
using ToFood.Domain.Services.TokenManager;

namespace ToFood.Domain.Services;

public class NotificationService : INotificationService
{

    private readonly ToFoodRelationalContext _dbRelationalContext;
    private readonly ILogger<NotificationService> _logger;
    private readonly EmailService _emailService;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;


    public NotificationService(
        ToFoodRelationalContext dbRelationalContext,
        ILogger<NotificationService> logger,
        EmailService emailService,
        Microsoft.Extensions.Configuration.IConfiguration configuration
        )
    {
        _dbRelationalContext = dbRelationalContext;
        _logger = logger;
        _emailService = emailService;
        _configuration = configuration;
    }

    /// <summary>
    /// Envia um email de notificação de arquivo.
    /// </summary>
    /// <param name="notificationId">ID da notificação.</param>
    /// <returns></returns>
    public async Task SendEmail(long notificationId)
    {
        try
        {
            // Busca a notificação pelo ID
            var fileNotification = await _dbRelationalContext.FileNotifications
                .AsNoTracking()
                .Where(x => x.Id == notificationId)
                .Where(x => x.Status == NotificationStatus.WaitingToBeSent)
                .Where(x => x.SentAt == null)
                .Select(n => new
                {
                    n.Id,
                    n.Email,
                    n.Text,
                    n.Subject,
                    n.TemplateText,
                    n.Attempt,
                    n.Status,
                    n.SentAt,
                    n.Operation,
                    n.Type,
                    n.FileId,
                    n.CreatedAt,
                    n.FileNotificationServiceId,
                    File = new
                    {
                        n.Video.FileName,
                        n.Video.FilePath
                    },
                    NotificationService = new
                    {
                        Id = n.FileNotificationServiceId,
                        ServiceName = n.FileNotificationService.Name,
                        ApiKey = n.FileNotificationService.ApiKey,
                        ApiEndpoint = n.FileNotificationService.ApiEndpoint
                    }
                })
                .FirstOrDefaultAsync() ?? throw new Exception($"A notificação {notificationId} não existe ou já foi enviada.");

            // Caso não tenha um serviço de notificação vinculado
            if (!fileNotification.NotificationService.Id.HasValue)
            {
                throw new Exception($"A notificação {notificationId} não possui um serviço de notificação configurado.");
            }

            // Verifica se o e-mail de destino é válido
            if (!fileNotification.Email.IsValidEmail())
            {
                // Atualiza a notificação como inválida
                await _dbRelationalContext.FileNotifications
                .UpdateAsync(notificationId, n => new()
                {
                    Status = NotificationStatus.NoValidContacts,
                    SentAt = DateTime.UtcNow
                });

                return;
            }

            // Prepara o corpo do e-mail com substituição de variáveis
            var notificationText = fileNotification.Text;
            var notificationTemplateText = fileNotification.TemplateText;


            // Realiza o envio do e-mail usando o serviço
            var emailResult = await _emailService.SendEmail(
                notificationServiceId: fileNotification.NotificationService.Id.Value,
                emails: [MailboxAddress.Parse(fileNotification.Email)],
                subject: fileNotification.Subject ?? "",
                htmlBody: notificationTemplateText ?? "",
                textBody: notificationText ?? "",
                attachments: null,
                emailsBcc: null

            );

            // Atualiza o status da notificação com base no resultado do envio
            if (emailResult.Success)
            {
                await _dbRelationalContext.FileNotifications
                .UpdateAsync(notificationId, n => new()
                {
                    Status = NotificationStatus.Success,
                    SentAt = DateTime.UtcNow
                });
            }
            else
            {
                await _dbRelationalContext.FileNotifications
                .UpdateAsync(notificationId, n => new()
                {
                    Status = NotificationStatus.Error,
                    SentAt = DateTime.UtcNow,
                    ErrorMessage = emailResult.ErrorMessage
                });

                throw new Exception($"Erro ao enviar o e-mail: {emailResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {

            await _dbRelationalContext.FileNotifications
                .UpdateAsync(notificationId, n => new()
                {
                    Status = NotificationStatus.Error,
                    SentAt = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                });

            _logger.LogError(ex, $"Erro ao processar a notificação de e-mail. Id da Notificação {notificationId} : {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Cria uma notificação no banco de dados e, em seguida, envia para a fila SQS.
    /// </summary>
    /// <param name="fileId">O ID do arquivo relacionado à notificação (opcional).</param>
    /// <returns></returns>
    public async Task CreateAndSendNotificationAsync(Guid fileId)
    {
        try
        {
            // Criação da notificação
            var notificationId = await CreateNotification(fileId);

            // Envio para a fila SQS
            await SendNotificationToSqs(notificationId);

            _logger.LogInformation($"Notificação {notificationId} criada e enviada para a fila SQS com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar ou enviar a notificação para a fila SQS.");
            throw;
        }
    }

    /// <summary>
    /// Cria uma notificação no banco de dados.
    /// </summary>
    /// <param name="fileId">O ID do arquivo relacionado à notificação (opcional).</param>
    /// <returns>O ID da notificação criada.</returns>
    internal async Task<long> CreateNotification(Guid fileId)
    {
        string email = "robert.ads.anjos@gmail.com"; // TODO: PEGAR DO TOKEN

        if (!email.IsValidEmail())
        {
            throw new ArgumentException("Email inválido ou vazio.", nameof(email));
        }

        var notificationService = await _dbRelationalContext.FileNotificationServices
            .AsNoTracking()
            .Where(fs => fs.IsActive)
            .Where(fs => fs.Type == NotificationType.Email)
            .FirstOrDefaultAsync();

        var notification = new FileNotification
        {
            Email = email,
            Subject = notificationService?.Title,
            Text = notificationService?.Text,
            TemplateText = notificationService?.TemplateText,
            Status = NotificationStatus.WaitingToBeSent,
            CreatedAt = DateTime.UtcNow,
            FileId = fileId,
            SentAt = null,
            FileNotificationServiceId = notificationService?.Id,
            Type = NotificationType.Email,
        };

        await _dbRelationalContext.FileNotifications.AddAsync(notification);
        await _dbRelationalContext.SaveChangesAsync();

        _logger.LogInformation($"Notificação {notification.Id} criada com sucesso.");
        return notification.Id;
    }

    /// <summary>
    /// Envia uma notificação para a fila SQS.
    /// </summary>
    /// <param name="notificationId">O ID da notificação.</param>
    public async Task SendNotificationToSqs(long notificationId)
    {
        try
        {
            // Recuperar as credenciais do Secrets Manager
            var sqsClient = AWSTokenManager.GetAWSClient(_configuration);

            // Construção da mensagem
            var messageBody = JsonSerializer.Serialize(new { NotificationId = notificationId });
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = _configuration["AWS:SQSQueueUrl"],
                MessageBody = messageBody
            };

            // Envio da mensagem
            var response = await sqsClient.SendMessageAsync(sendMessageRequest);

            // Log do sucesso
            _logger.LogInformation($"Notificação {notificationId} enviada para a fila SQS com sucesso. ID da Mensagem: {response.MessageId}");
        }
        catch (Exception ex)
        {
            // Log de erro
            _logger.LogError(ex, $"Erro ao enviar a notificação {notificationId} para a fila SQS.");
            throw;
        }
    }

}
