using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ToFood.Domain.Enums;

namespace ToFood.Domain.Entities.Relational;

/// <summary>
/// Representa um vídeo enviado pelo usuário.
/// </summary>
[Table("videos")]
public class Video
{
    /// <summary>
    /// Identificador único do vídeo.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Nome do arquivo do vídeo.
    /// </summary>
    [MaxLength(255, ErrorMessage = "O nome do arquivo não pode exceder 255 caracteres.")]
    [Column("file_name")]
    public string? FileName { get; set; }

    /// <summary>
    /// Caminho completo onde o arquivo está armazenado.
    /// </summary>
    [MaxLength(1024, ErrorMessage = "O caminho do arquivo não pode exceder 1024 caracteres.")]
    [Column("file_path")]
    public string? FilePath { get; set; }

    /// <summary>
    /// Status atual do vídeo (ex.: Processando, Concluído).
    /// </summary>
    [Required(ErrorMessage = "O status do vídeo é obrigatório.")]
    [Column("status")]
    public VideoStatus Status { get; set; }

    /// <summary>
    /// Identificador único do usuário associado ao vídeo.
    /// </summary>
    [Required(ErrorMessage = "O identificador do usuário é obrigatório.")]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Relacionamento com a entidade de usuário que enviou o vídeo.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Relacionamento com os arquivos ZIP associados ao vídeo.
    /// </summary>
    public ICollection<ZipFile>? ZipFiles { get; set; }

    /// <summary>
    /// Data de criação do registro do vídeo.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização do registro do vídeo.
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Dados binários do vídeo.
    /// </summary>
    [Column("file_data")]
    public byte[]? FileData { get; set; }
}
