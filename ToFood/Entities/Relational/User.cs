﻿using System.ComponentModel.DataAnnotations;
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
    public int Id { get; set; }

    /// <summary>
    /// Email do usuário.
    /// </summary>
    [Required(ErrorMessage = "O email do usuário é obrigatório.")]
    [MaxLength(255, ErrorMessage = "O email do usuário não pode exceder 255 caracteres.")]
    [Column("email")]
    public string? Email { get; set; }

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
}