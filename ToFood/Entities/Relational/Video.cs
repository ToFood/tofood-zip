using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Compression;
using ToFood.Domain.Enums;

namespace ToFood.Domain.Entities.Relational;

/// <summary>
/// Representa um vídeo enviado pelo usuário.
/// </summary>
[Table("videos")]
public class Video
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("file_name")]
    public string FileName { get; set; }

    [Required]
    [MaxLength(1024)]
    [Column("file_path")]
    public string FilePath { get; set; }

    [Required]
    [Column("status")]
    public VideoStatus Status { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    // Relacionamento com o usuário
    public User User { get; set; }

    // Relacionamento com arquivos ZIP
    public ICollection<ZipFile> ZipFiles { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
