using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Services;

namespace ToFood.Tests.UnitTests;

/// <summary>
/// Testes unitários para o serviço de autenticação (UserService).
/// </summary>
public class UserServiceUnitTests : TestBase
{
    private readonly UserService _userService;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Construtor que inicializa a classe de testes.
    /// </summary>
    public UserServiceUnitTests()
    {
        // Instanciar o logger real ou um mock
        _logger = new LoggerFactory().CreateLogger<UserService>();

        // Instância do UserService usando o contexto herdado de TestBase
        _userService = new UserService(RelationalContext, _logger);
    }

    /// <summary>
    /// Testa o registro de um novo usuário com sucesso.
    /// </summary>
    [Fact]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Act
        var response = await _userService.Register("newuser@example.com", "password123");

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal("Usuário registrado com sucesso!", response.Message);

        var user = await RelationalContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        Assert.NotNull(user); // Garante que o usuário foi adicionado ao banco
    }

    /// <summary>
    /// Testa o registro de um usuário com email já existente.
    /// </summary>
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsError()
    {
        // Arrange
        RelationalContext.Users.Add(new User { Email = "existinguser@example.com", PasswordHash = "hash" });
        await RelationalContext.SaveChangesAsync();

        // Act
        var response = await _userService.Register("existinguser@example.com", "password123");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("Email já está em uso.", response.Message);
    }
}
