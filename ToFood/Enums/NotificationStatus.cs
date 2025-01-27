namespace ToFood.Domain.Enums;

public enum NotificationStatus
{
    /// <summary>
    /// Aguardando para ser enviado
    /// </summary>
    WaitingToBeSent = 1,

    /// <summary>
    /// Sucesso
    /// </summary>
    Success = 2,

    /// <summary>
    /// Erro Genérico
    /// </summary>
    Error = 3,

    /// <summary>
    /// Sem e-mail válido
    /// </summary>
    NoValidContacts = 4,

    /// <summary>
    /// Não enviada
    /// </summary>
    NotSent = 5,
}
