using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Enums;
using ToFood.Domain.Extensions;
using ToFood.Domain.Services.Notifications;

namespace ToFood.Domain.Services;

public class NotificationService
{

    private readonly ToFoodRelationalContext _dbRelationalContext;
    private readonly ILogger<NotificationService> _logger;
    private readonly EmailService _emailService;

    public NotificationService(ToFoodRelationalContext dbRelationalContext, ILogger<NotificationService> logger, EmailService emailService)
    {
        _dbRelationalContext = dbRelationalContext;
        _logger = logger;
        _emailService = emailService;
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
            if (string.IsNullOrWhiteSpace(fileNotification.Email) || !fileNotification.Email.IsValidEmail())
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
}
