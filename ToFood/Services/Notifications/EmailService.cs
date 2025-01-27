using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Enums;

namespace ToFood.Domain.Services.Notifications;

/// <summary>
/// Serviço responsável pelo envio de e-mails.
/// </summary>
public class EmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly ToFoodRelationalContext _dbRelationalContext;

    public EmailService(ILogger<EmailService> logger, ToFoodRelationalContext dbRelationalContext)
    {
        _logger = logger;
        _dbRelationalContext = dbRelationalContext;
    }



    /// <summary>
    /// Envia um e-mail utilizando o serviço configurado no banco de dados.
    /// </summary>
    /// <param name="notificationServiceId">ID do serviço de notificação.</param>
    /// <param name="emails">Lista de destinatários.</param>
    /// <param name="subject">Assunto do e-mail.</param>
    /// <param name="htmlBody">Corpo do e-mail em formato HTML.</param>
    /// <param name="textBody">Corpo do e-mail em formato texto.</param>
    /// <param name="attachments">Anexos do e-mail (opcional).</param>
    /// <param name="emailsBcc">Lista de destinatários em cópia oculta (opcional).</param>
    /// <returns>Resultado da operação de envio.</returns>
    public async Task<EmailResult> SendEmail(
        Guid notificationServiceId,
        MailboxAddress[] emails,
        string subject,
        string htmlBody,
        string textBody,
        AttachmentCollection? attachments = null,
        List<MailboxAddress>? emailsBcc = null)
    {
        ValidateInputs(notificationServiceId, emails);

        attachments ??= new AttachmentCollection();
        var body = CreateEmailBody(htmlBody, textBody, attachments);
        var notificationService = await GetNotificationService(notificationServiceId);
        var message = CreateEmailMessage(notificationService, emails, subject, body, emailsBcc);

        var emailResult = await SendEmailUsingBroker(notificationService, message, notificationServiceId);
        await Task.Delay(notificationService.MillisecondsBetweenNotifications);

        return emailResult;
    }

    /// <summary>
    /// Valida os parâmetros principais para envio de e-mail.
    /// </summary>
    private void ValidateInputs(Guid? notificationServiceId, MailboxAddress[] emails)
    {
        if (!notificationServiceId.HasValue)
            throw new ArgumentException("Serviço de e-mail não informado. ERPOOOES32.");

        if (emails == null || emails.Length == 0)
            throw new ArgumentException("Nenhum destinatário de e-mail para enviar. ERASKJHS2.");
    }

    /// <summary>
    /// Cria o corpo do e-mail com texto, HTML e anexos.
    /// </summary>
    private static BodyBuilder CreateEmailBody(string htmlBody, string textBody, AttachmentCollection attachments)
    {
        var body = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody
        };

        foreach (var attachment in attachments)
        {
            body.Attachments.Add(attachment);
        }

        return body;
    }

    /// <summary>
    /// Recupera os detalhes do serviço de notificação pelo ID.
    /// </summary>
    private async Task<dynamic> GetNotificationService(Guid notificationServiceId)
    {
        var notificationService = await _dbRelationalContext.FileNotificationServices
            .AsNoTracking()
            .Where(x => x.Id == notificationServiceId)
            .Select(t => new
            {
                t.Id,
                t.EmailSender,
                t.EmailSenderName,
                t.EmailReplyTo,
                t.EmailCCO,
                t.Broker,
                t.MillisecondsBetweenNotifications,
                t.ApiKey,
                t.ApiSecret,
                t.ApiEndpoint,
                t.IgnoreSsl
            })
            .FirstOrDefaultAsync();

        if (notificationService == null || string.IsNullOrWhiteSpace(notificationService.EmailSender))
            throw new Exception("Serviço de notificação ou remetente inválido. ERAS54X1L.");

        return notificationService;
    }

    /// <summary>
    /// Monta a mensagem de e-mail com os detalhes do remetente, destinatários e corpo.
    /// </summary>
    private MimeMessage CreateEmailMessage(dynamic notificationService, MailboxAddress[] emails, string subject, BodyBuilder body, List<MailboxAddress> emailsBcc)
    {
        var message = new MimeMessage
        {
            Subject = subject,
            Body = body.ToMessageBody()
        };

        message.From.Add(new MailboxAddress(notificationService.EmailSenderName, notificationService.EmailSender.Trim()));
        message.ReplyTo.Add(new MailboxAddress(notificationService.EmailSenderName, string.IsNullOrWhiteSpace(notificationService.EmailReplyTo) ? notificationService.EmailSender : notificationService.EmailReplyTo.Trim()));
        message.To.AddRange(emails);

        if (!string.IsNullOrWhiteSpace(notificationService.EmailCCO))
            message.Bcc.Add(new MailboxAddress("", notificationService.EmailCCO));

        if (emailsBcc?.Any() == true)
            message.Bcc.AddRange(emailsBcc);

        return message;
    }

    /// <summary>
    /// Realiza o envio do e-mail utilizando o broker configurado.
    /// </summary>
    private async Task<EmailResult> SendEmailUsingBroker(dynamic notificationService, MimeMessage message, Guid notificationServiceId)
    {
        return notificationService.Broker switch
        {
            NotificationServiceBroker.Smtp => await SendUsingSmtp(notificationService, message),
            _ => throw new Exception("Integração de e-mail não encontrada.")
        };
    }

    /// <summary>
    /// Envia o e-mail utilizando o protocolo SMTP.
    /// </summary>
    private async Task<EmailResult> SendUsingSmtp(dynamic notificationService, MimeMessage message)
    {
        try
        {
            using var smtpClient = new SmtpClient();
            if (notificationService.IgnoreSsl)
                smtpClient.ServerCertificateValidationCallback = (_, _, _, _) => true;

            await smtpClient.ConnectAsync(notificationService.ApiEndpoint ?? "smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(notificationService.ApiKey, notificationService.ApiSecret);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);

            return new EmailResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail.");
            return new EmailResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}

/// <summary>
/// Representa o resultado do envio de e-mail.
/// </summary>
public class EmailResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
