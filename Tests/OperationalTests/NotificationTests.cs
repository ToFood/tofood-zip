using Microsoft.Extensions.Logging;
using ToFood.Domain.Services;
using ToFood.Domain.Services.Notifications;

namespace ToFood.Tests.OperationalTests;

/// <summary>
/// Testes operacionais para o NotificationService.
/// </summary>
public class NotificationServiceOperationalTests : TestOperationalBase
{
    private readonly NotificationService _notificationService;

    /// <summary>
    /// Construtor que inicializa a classe de testes operacionais.
    /// </summary>
    public NotificationServiceOperationalTests()
    {
        // Configurar o serviço de notificação
        var loggerEmailService = new LoggerFactory().CreateLogger<EmailService>();
        var emailService = new EmailService(loggerEmailService, RelationalContext);

        var logger = new LoggerFactory().CreateLogger<NotificationService>();


        _notificationService = new NotificationService(RelationalContext, logger, emailService);
    }

    /// <summary>
    /// Enviar uma notificação específica de forma manual (operacional).
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendEmailTest()
    {
        // ID de notificação existente no banco real
        long notificationId = 1;

        // Executar o método de envio de notificação
        await _notificationService.SendEmail(notificationId);

        Console.WriteLine("Notificação enviada com sucesso!");
    }

    /// <summary>
    /// Enviar uma notificação específica de forma manual (operacional).
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateNotificationTest()
    {
        // Executar o método de envio de notificação
        await _notificationService.CreateNotification(new Guid("732eeeef-8a09-4a3f-90af-6dcb67a9fcad"));

        Console.WriteLine("Notificação criada com sucesso!");
    }
}
