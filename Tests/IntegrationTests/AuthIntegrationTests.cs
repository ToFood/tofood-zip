using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Services;

namespace ToFood.Tests.IntegrationTests;

/// <summary>
/// Classe de testes de integração para o serviço de autenticação (AuthService).
/// </summary>
public class AuthIntegrationTests : TestBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Construtor que inicializa a classe de testes.
    /// </summary>
    public AuthIntegrationTests()
    {
        // Instanciar o logger real ou um mock
        _logger = new LoggerFactory().CreateLogger<AuthService>();

        // Inicializa o AuthService com o contexto e a configuração herdados de TestBase
        _authService = new AuthService(RelationalContext, _logger, Configuration);
    }

    /// <summary>
    /// Popula o banco de dados com dados iniciais para os testes.
    /// </summary>
    protected override void SeedDatabase()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");

        RelationalContext.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash = passwordHash
        });

        RelationalContext.SaveChanges();
    }




    /// <summary>
    /// Testa o login de um usuário com credenciais válidas.
    /// </summary>
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Act
        var response = await _authService.Login("test@example.com", "password123");

        // Assert
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Token);
        Assert.Equal("Login bem-sucedido!", response.Message);
    }

    /// <summary>
    /// Testa o login de um usuário com credenciais inválidas (email errado).
    /// </summary>
    [Fact]
    public async Task Login_InvalidEmail_ReturnsError()
    {
        // Act
        var response = await _authService.Login("invalid@example.com", "password123");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("E-mail ou senha inválidos.", response.Message);
    }

    /// <summary>
    /// Testa o login de um usuário com senha incorreta.
    /// </summary>
    [Fact]
    public async Task Login_InvalidPassword_ReturnsError()
    {
        // Act
        var response = await _authService.Login("test@example.com", "wrongpassword");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("E-mail ou senha inválidos.", response.Message);
    }

}
