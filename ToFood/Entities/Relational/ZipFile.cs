using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ToFood.Domain.Enums;

namespace ToFood.Domain.Entities.Relational;

/// <summary>
/// Representa um arquivo ZIP gerado a partir dos frames de um vídeo.
/// </summary>
[Table("zip_files")]
public class ZipFile
{
    /// <summary>
    /// Identificador único do arquivo ZIP.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Caminho completo onde o arquivo ZIP está armazenado.
    /// </summary>
    [Required(ErrorMessage = "O caminho do arquivo ZIP é obrigatório.")]
    [MaxLength(1024, ErrorMessage = "O caminho do arquivo ZIP não pode exceder 1024 caracteres.")]
    [Column("file_path")]
    public string? FilePath { get; set; }

    /// <summary>
    /// Status atual do arquivo ZIP (ex.: Processando, Concluído).
    /// </summary>
    [Required(ErrorMessage = "O status do arquivo ZIP é obrigatório.")]
    [Column("status")]
    public ZipStatus Status { get; set; }

    /// <summary>
    /// Identificador único do vídeo associado ao arquivo ZIP.
    /// </summary>
    [Required(ErrorMessage = "O identificador do vídeo é obrigatório.")]
    [Column("video_id")]
    public Guid VideoId { get; set; }

    /// <summary>
    /// Relacionamento com a entidade de vídeo associada ao arquivo ZIP.
    /// </summary>
    public Video? Video { get; set; }

    /// <summary>
    /// Data de criação do registro do arquivo ZIP.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização do registro do arquivo ZIP.
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identificador único do usuário associado ao arquivo ZIP.
    /// </summary>
    [Required(ErrorMessage = "O identificador do usuário é obrigatório.")]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Relacionamento com a entidade de usuário associada ao arquivo ZIP.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Dados binários do arquivo ZIP.
    /// </summary>
    [Column("file_data")]
    public byte[]? FileData { get; set; }

}
