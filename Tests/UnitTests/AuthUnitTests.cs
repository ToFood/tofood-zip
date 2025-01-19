using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Services;

namespace ToFood.Tests.UnitTests;

/// <summary>
/// Testes unitários para o serviço de autenticação (AuthService).
/// </summary>
public class AuthServiceUnitTests
{
    private readonly ToFoodRelationalContext _dbContext;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceUnitTests()
    {
        // Configurar o banco de dados In-Memory
        var options = new DbContextOptionsBuilder<ToFoodRelationalContext>()
            .UseInMemoryDatabase("AuthUnitTestDatabase")
            .Options;

        _dbContext = new ToFoodRelationalContext(options);

        // Adicionar configuração in-memory para JWT
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "tofood!aA1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V2W3X4Y5Z6!" },
                { "Jwt:Issuer", "your-issuer" },
                { "Jwt:Audience", "your-audience" }
            })
            .Build();

        // Instanciar o logger real ou um mock
        _logger = new LoggerFactory().CreateLogger<AuthService>();

        // Instância do AuthService
        _authService = new AuthService(_dbContext, _logger, _configuration);
    }

    /// <summary>
    /// Testa o login de um usuário com credenciais válidas.
    /// </summary>
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        _dbContext.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await _dbContext.SaveChangesAsync();

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
    [Fact]
    public async Task Login_InvalidEmail_ReturnsError()
    {
        // Arrange
        _dbContext.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _authService.Login("invalid@example.com", "password123");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("E-mail ou senha inválidos.", response.Message);
    }

    /// <summary>
    /// Testa o login de um usuário com senha inválida.
    /// </summary>
    [Fact]
    public async Task Login_InvalidPassword_ReturnsError()
    {
        // Arrange
        _dbContext.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _authService.Login("test@example.com", "wrongpassword");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("E-mail ou senha inválidos.", response.Message);
    }
}
