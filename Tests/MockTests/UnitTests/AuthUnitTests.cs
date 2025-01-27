using Microsoft.Extensions.Logging;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Services;

namespace ToFood.Tests.MockTests.UnitTests;

/// <summary>
/// Testes unitários para o serviço de autenticação (AuthService).
/// </summary>
public class AuthServiceUnitTests : TestMockBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Construtor que inicializa a classe de testes.
    /// </summary>
    public AuthServiceUnitTests()
    {
        // Instanciar o logger real ou um mock
        _logger = new LoggerFactory().CreateLogger<AuthService>();

        // Instância do AuthService usando o contexto e a configuração herdados de TestBase
        _authService = new AuthService(RelationalContext, _logger, Configuration);
    }

    /// <summary>
    /// Testa o login de um usuário com credenciais válidas.
    /// </summary>
    [Trait("Category", "Unit")]
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        RelationalContext.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await RelationalContext.SaveChangesAsync();

        // Act
        var response = await _authService.Login("test@example.com", "password123");

        // Assert
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Token);
        Assert.Equal("Login bem-sucedido!", response.Message);
    }

    /// <summary>
    /// Testa o login de um usuário com email inválido.
    /// </summary>
    [Trait("Category", "Unit")]
    [Fact]
    public async Task Login_InvalidEmail_ReturnsError()
    {
        // Arrange
        RelationalContext.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await RelationalContext.SaveChangesAsync();

        // Act
        var response = await _authService.Login("invalid@example.com", "password123");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("E-mail ou senha inválidos.", response.Message);
    }

    /// <summary>
    /// Testa o login de um usuário com senha inválida.
    /// </summary>
    [Trait("Category", "Unit")]
    [Fact]
    public async Task Login_InvalidPassword_ReturnsError()
    {
        // Arrange
        RelationalContext.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await RelationalContext.SaveChangesAsync();

        // Act
        var response = await _authService.Login("test@example.com", "wrongpassword");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("E-mail ou senha inválidos.", response.Message);
    }
}
