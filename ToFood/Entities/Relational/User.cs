using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToFood.Domain.Entities.Relational;

/// <summary>
/// Representa um usuário do sistema.
/// </summary>
[Table("users")]
public class User
{
    /// <summary>
    /// Identificador único do usuário.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Nome completo do usuário.
    /// </summary>
    [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
    [MaxLength(255, ErrorMessage = "O nome do usuário não pode exceder 255 caracteres.")]
    [Column("full_name")]
    public string? FullName { get; set; }

    /// <summary>
    /// Email do usuário.
    /// </summary>
    [Required(ErrorMessage = "O email do usuário é obrigatório.")]
    [EmailAddress(ErrorMessage = "O email informado não é válido.")]
    [MaxLength(255, ErrorMessage = "O email do usuário não pode exceder 255 caracteres.")]
    [Column("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Telefone do usuário.
    /// </summary>
    [MaxLength(20, ErrorMessage = "O telefone não pode exceder 20 caracteres.")]
    [Column("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Hash da senha do usuário.
    /// </summary>
    [Required(ErrorMessage = "O hash da senha é obrigatório.")]
    [MaxLength(255, ErrorMessage = "O hash da senha não pode exceder 255 caracteres.")]
    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Data de criação do usuário.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização do registro.
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Relacionamento com os vídeos enviados pelo usuário.
    /// </summary>
    public ICollection<Video>? Videos { get; set; }
}
