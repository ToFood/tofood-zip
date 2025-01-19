using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Services;
using ToFood.Domain.DB.Relational;

namespace ToFood.Tests.IntegrationTests;

/// <summary>
/// Classe de testes de integração para o serviço de autenticação (AuthService).
/// </summary>
public class AuthIntegrationTests
{
    private readonly AuthService _authService;
    private readonly ToFoodRelationalContext _dbContext;

    /// <summary>
    /// Construtor que inicializa a classe de testes.
    /// </summary>
    public AuthIntegrationTests()
    {
        // Configurar o banco de dados In-Memory
        var options = new DbContextOptionsBuilder<ToFoodRelationalContext>()
            .UseInMemoryDatabase("AuthIntegrationTestDatabase")
            .Options;

        _dbContext = new ToFoodRelationalContext(options);

        // Configuração do IConfiguration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "tofood!aA1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V2W3X4Y5Z6!" },
                { "Jwt:Issuer", "your-issuer" },
                { "Jwt:Audience", "your-audience" }
            })
            .Build();

        // Logger real ou mock
        var logger = new LoggerFactory().CreateLogger<AuthService>();

        // Inicializa o AuthService com o contexto e as dependências configuradas
        _authService = new AuthService(_dbContext, logger, configuration);

        // Popula o banco de dados com dados iniciais
        SeedDatabase();
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

    /// <summary>
    /// Popula o banco de dados com dados iniciais para os testes.
    /// </summary>
    private void SeedDatabase()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");

        _dbContext.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash = passwordHash
        });

        _dbContext.SaveChanges();
    }
}
