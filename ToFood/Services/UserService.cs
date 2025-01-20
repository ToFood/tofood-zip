using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.DTOs.Request;



namespace ToFood.Domain.Services;

/// <summary>
/// Serviço responsável por autenticação, registro de usuários
/// </summary>
public class UserService
{
    private readonly ToFoodRelationalContext _dbRelationalContext;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Inicializa uma nova instância do serviço de autenticação.
    /// </summary>
    public UserService(ToFoodRelationalContext dbRelationalContext, ILogger<UserService> logger)
    {
        _dbRelationalContext = dbRelationalContext;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo usuário no sistema.
    /// </summary>
    /// <param name="fullName">O nome completo do usuário.</param>
    /// <param name="email">O email do usuário.</param>
    /// <param name="phone">O telefone do usuário.</param>
    /// <param name="password">A senha do usuário.</param>
    /// <returns>Um objeto contendo o resultado da operação.</returns>
    public async Task<UserResponse> Register(RegisterUserRequest registerUserRequest)
    {
        if (string.IsNullOrWhiteSpace(registerUserRequest.Email))
        {
            // Lógica para tratar email nulo, vazio ou composto apenas por espaços
            _logger.LogWarning("Falha no registro: Email não fornecido ou inválido | @Request: {Request}", RequestSanitizer.Sanitize(registerUserRequest));
            throw new ArgumentException("O email não pode ser nulo ou vazio.");
        }

        if (string.IsNullOrWhiteSpace(registerUserRequest.Password))
        {
            // Lógica para tratar senha nula, vazia ou composta apenas por espaços
            _logger.LogWarning("Falha no registro: Email não fornecido ou inválido | @Request: {Request}", RequestSanitizer.Sanitize(registerUserRequest));
            throw new ArgumentException("A senha não pode ser nula ou vazia.");
        }

        _logger.LogInformation("Iniciando registro para @Email {Email} | @Request: {Request}", registerUserRequest.Email, RequestSanitizer.Sanitize(registerUserRequest));

        // Verifica se o email já está em uso
        if (await _dbRelationalContext.Users.AnyAsync(u => u.Email == registerUserRequest.Email))
        {
            _logger.LogWarning("Falha no registro: @Email {Email} já está em uso | @Request: {Request}", registerUserRequest.Email, RequestSanitizer.Sanitize(registerUserRequest));
            return new UserResponse { IsSuccess = false, Message = "Email já está em uso." };
        }

        // Criação do hash de senha e instância do usuário
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserRequest.Password);
        var user = new User
        {
            FullName = registerUserRequest.FullName,
            Email = registerUserRequest.Email,
            Phone = registerUserRequest.Phone,
            PasswordHash = passwordHash
        };

        try
        {
            // Salva o usuário no banco de dados
            _dbRelationalContext.Users.Add(user);
            await _dbRelationalContext.SaveChangesAsync();

            _logger.LogInformation("Registro concluído para @Email {Email} | @Request: {Request}", registerUserRequest.Email, RequestSanitizer.Sanitize(registerUserRequest));
            return new UserResponse { IsSuccess = true, Message = "Usuário registrado com sucesso!" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar @Email {Email} | @Request: {Request} | @ErrorMessage: {ErrorMessage}", registerUserRequest.Email, RequestSanitizer.Sanitize(registerUserRequest), ex.Message);
            return new UserResponse { IsSuccess = false, Message = "Erro interno no servidor." };
        }
    }

    /// <summary>
    /// Retorna uma lista de usuários registrados no sistema.
    /// </summary>
    /// <returns>Uma lista de usuários com seus IDs, nomes, emails, telefones e datas de criação.</returns>
    public async Task<object> GetUsers()
    {
        _logger.LogInformation("Obtendo lista de usuários");

        var users = await _dbRelationalContext.Users
            .Select(u => new { u.Id, u.FullName, u.Email, u.Phone, u.CreatedAt })
            .ToListAsync();

        _logger.LogInformation("Lista de usuários obtida com sucesso | @TotalUsers: {UserCount}", users.Count);
        return users;
    }
}

/// <summary>
/// Representa a resposta das operações de autenticação.
/// </summary>
public class UserResponse
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Mensagem de retorno da operação.
    /// </summary>
    public string? Message { get; set; }
}
