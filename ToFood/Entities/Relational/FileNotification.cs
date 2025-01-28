using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ToFood.Domain.Enums;

namespace ToFood.Domain.Entities.Relational;

/// <summary>
/// Representa uma notificação de arquivo.
/// </summary>
[Table("file_notifications")]
public class FileNotification
{
    /// <summary>
    /// Identificador único da notificação.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Gera o valor automaticamente
    [Column("id")] // Nome da coluna no banco
    public long Id { get; set; }

    /// <summary>
    /// Tipo da notificação.
    /// </summary>
    [Required]
    [Column("type")]
    public NotificationType Type { get; set; }

    /// <summary>
    /// Data de envio da notificação.
    /// </summary>
    [Column("sent_at")]
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Status da notificação.
    /// </summary>
    [Required]
    [Column("status")]
    public NotificationStatus Status { get; set; }

    /// <summary>
    /// Assunto da notificação.
    /// </summary>
    [Column("subject")]
    public string? Subject { get; set; }

    /// <summary>
    /// Texto da notificação.
    /// </summary>
    [Column("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Identificador do arquivo associado à notificação.
    /// </summary>
    [Required]
    [Column("file_id")]
    public Guid FileId { get; set; }

    /// <summary>
    /// Relacionamento com a entidade de vídeo (arquivo associado).
    /// </summary>
    public Video? Video { get; set; }

    /// <summary>
    /// Flag indicando se a notificação foi excluída.
    /// </summary>
    [Required]
    [Column("deleted")]
    public bool Deleted { get; set; } = false;

    /// <summary>
    /// Data de criação do registro.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// URL da imagem do cabeçalho da notificação.
    /// </summary>
    [Column("header_image_url")]
    public string? HeaderImageUrl { get; set; }

    /// <summary>
    /// Endereço de e-mail do destinatário.
    /// </summary>
    [Column("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Número de telefone do destinatário.
    /// </summary>
    [Column("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Operação realizada na notificação.
    /// </summary>
    [Column("operation")]
    public string? Operation { get; set; }

    /// <summary>
    /// Mensagem de erro associada à notificação.
    /// </summary>
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Identificador do serviço de notificação associado.
    /// </summary>
    [Column("file_notification_service_id")]
    public Guid? FileNotificationServiceId { get; set; }

    /// <summary>
    /// Relacionamento com o serviço de notificação associado.
    /// </summary>
    public FileNotificationService? FileNotificationService { get; set; }

    /// <summary>
    /// Número de tentativas de envio da notificação.
    /// </summary>
    [Required]
    [Column("attempt")]
    public int Attempt { get; set; } = 1;

    /// <summary>
    /// Texto do template associado à notificação.
    /// </summary>
    [Column("template_text")]
    public string? TemplateText { get; set; }
}
