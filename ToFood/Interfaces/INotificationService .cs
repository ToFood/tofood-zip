namespace ToFood.Domain.Interfaces;

/// <summary>
/// Interface para serviços de notificação.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Envia um email de notificação baseado no ID fornecido.
    /// </summary>
    /// <param name="notificationId">ID da notificação.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    Task SendEmail(long notificationId);
}
