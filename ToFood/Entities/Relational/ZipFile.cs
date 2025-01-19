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
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(1024)]
    [Column("file_path")]
    public string FilePath { get; set; }

    [Required]
    [Column("status")]
    public ZipStatus Status { get; set; }

    [Required]
    [Column("video_id")]
    public Guid VideoId { get; set; }

    // Relacionamento com o vídeo
    public Video Video { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
