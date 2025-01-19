namespace ToFood.Domain.DTOs.Request;

/// <summary>
/// Representa os dados necessários para registrar um novo usuário.
/// </summary>
public class RegisterUserRequest
{
    /// <summary>
    /// ID da Requisição
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Nome completo do usuário.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Email do usuário.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Telefone do usuário (opcional).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Senha do usuário.
    /// </summary>
    public string? Password { get; set; }
}
