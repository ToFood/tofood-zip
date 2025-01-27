using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ToFood.Domain.Enums;

namespace ToFood.Domain.Entities.Relational;

/// <summary>
/// Representa um serviço de notificação de arquivos.
/// </summary>
[Table("file_notification_services")]
public class FileNotificationService
{
    /// <summary>
    /// Identificador único do serviço.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo do serviço de notificação.
    /// </summary>
    [Required]
    [Column("type")]
    public NotificationType Type { get; set; }

    /// <summary>
    /// Nome do serviço de notificação.
    /// </summary>
    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// API Key para autenticação do serviço.
    /// </summary>
    [Column("api_key")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// API Secret para autenticação do serviço.
    /// </summary>
    [Column("api_secret")]
    public string? ApiSecret { get; set; }

    /// <summary>
    /// Broker do serviço de notificação.
    /// </summary>
    [Required]
    [Column("broker")]
    public NotificationServiceBroker Broker { get; set; }

    /// <summary>
    /// Endpoint da API do serviço de notificação.
    /// </summary>
    [Column("api_endpoint")]
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// Indica se o serviço está ativo.
    /// </summary>
    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Tempo em milissegundos entre notificações.
    /// </summary>
    [Required]
    [Column("milliseconds_between_notifications")]
    public int MillisecondsBetweenNotifications { get; set; } = 0;

    /// <summary>
    /// Indica se a validação de SSL deve ser ignorada.
    /// </summary>
    [Required]
    [Column("ignore_ssl")]
    public bool IgnoreSsl { get; set; } = false;

    /// <summary>
    /// Tipo da requisição utilizada pelo serviço.
    /// </summary>
    [Column("request_type")]
    public string? RequestType { get; set; }

    /// <summary>
    /// Cabeçalhos da requisição em formato JSON.
    /// </summary>
    [Column("request_headers")]
    public string? RequestHeaders { get; set; }

    /// <summary>
    /// E-mail remetente padrão.
    /// </summary>
    [Column("email_sender")]
    public string? EmailSender { get; set; }

    /// <summary>
    /// E-mail para respostas (Reply-To).
    /// </summary>
    [Column("email_reply_to")]
    public string? EmailReplyTo { get; set; }

    /// <summary>
    /// E-mails de cópia oculta (BCC).
    /// </summary>
    [Column("email_cco")]
    public string? EmailCCO { get; set; }

    /// <summary>
    /// Indica se o e-mail deve ser assinado.
    /// </summary>
    [Required]
    [Column("should_sign_email")]
    public bool ShouldSignEmail { get; set; } = false;

    /// <summary>
    /// Nome do remetente do e-mail.
    /// </summary>
    [Column("email_sender_name")]
    public string? EmailSenderName { get; set; }

    /// <summary>
    /// Relacionamento com as notificações associadas ao serviço.
    /// </summary>
    public ICollection<FileNotification>? FileNotifications { get; set; }
}
