using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Entities.Relational;

namespace ToFood.Domain.Services;

/// <summary>
/// Serviço responsável por autenticação, registro de usuários e geração de tokens JWT.
/// </summary>
public class AuthService
{
    private readonly ToFoodRelationalContext _dbRelationalContext;
    private readonly ILogger<AuthService> _logger;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    /// <summary>
    /// Inicializa uma nova instância do serviço de autenticação.
    /// </summary>
    public AuthService(
        ToFoodRelationalContext dbRelationalContext,
        ILogger<AuthService> logger,
        Microsoft.Extensions.Configuration.IConfiguration configuration // Alterado para IConfiguration
        )
    {
        _dbRelationalContext = dbRelationalContext;
        _logger = logger;
        _configuration = configuration;
    }


    /// <summary>
    /// Realiza o login de um usuário.
    /// </summary>
    /// <param name="email">O email do usuário.</param>
    /// <param name="password">A senha do usuário.</param>
    /// <returns>Um objeto contendo o resultado da operação.</returns>
    public async Task<AuthResponse> Login(string email, string password)
    {
        var requestId = Guid.NewGuid().ToString(); // Identificador único para rastrear a requisição
        _logger.LogInformation("Iniciando login para usuário @{Email} | RequestId: @{RequestId}", email, requestId);

        // Busca o usuário no banco de dados
        var user = await _dbRelationalContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Falha no login: Credenciais inválidas para usuário @{Email} | RequestId: @{RequestId}", email, requestId);
            return new AuthResponse { IsSuccess = false, Message = "E-mail ou senha inválidos." };
        }

        // Gera o token JWT para o usuário
        var token = GenerateJwtToken(user);

        _logger.LogInformation("Login bem-sucedido para usuário {Email} | UserId: @{UserId} | RequestId: @{RequestId}", email, user.Id, requestId);
        return new AuthResponse { IsSuccess = true, Message = "Login bem-sucedido!", Token = token };
    }

    /// <summary>
    /// Gera um token JWT para autenticar o usuário.
    /// </summary>
    /// <param name="user">O usuário para o qual o token será gerado.</param>
    /// <returns>O token JWT gerado.</returns>
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user?.Email ?? ""),
                new Claim("userId", user?.Id.ToString() ?? ""),
                new Claim("userEmail", user?.Email ?? ""),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"], // Adiciona emissor
            Audience = _configuration["Jwt:Audience"], // Adiciona audiência
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        _logger.LogDebug("Token gerado para usuário @{UserId} | Expiração: @{Expiration}", user?.Id, tokenDescriptor.Expires);
        return tokenHandler.WriteToken(token);
    }
}

/// <summary>
/// Representa a resposta das operações de autenticação.
/// </summary>
public class AuthResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
}
