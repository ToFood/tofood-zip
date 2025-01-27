using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.DTOs.Request;
using ToFood.Domain.Services;

namespace ToFood.Tests.MockTests.UnitTests;

/// <summary>
/// Testes unitários para o serviço de autenticação (UserService).
/// </summary>
public class UserServiceUnitTests : TestMockBase
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
    [Trait("Category", "Unit")]
    [Fact]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var registerUserRequest = new RegisterUserRequest
        {
            FullName = "João Silva",
            Email = "newuser@example.com",
            Phone = "+5511999999999",
            Password = "password123"
        };

        // Act
        var response = await _userService.Register(registerUserRequest);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal("Usuário registrado com sucesso!", response.Message);

        var user = await RelationalContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        Assert.NotNull(user); // Garante que o usuário foi adicionado ao banco
        Assert.Equal(registerUserRequest.FullName, user.FullName);
        Assert.Equal(registerUserRequest.Phone, user.Phone);
    }

    /// <summary>
    /// Testa o registro de um usuário com email já existente.
    /// </summary>
    [Trait("Category", "Unit")]
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsError()
    {
        // Arrange
        RelationalContext.Users.Add(new User
        {
            FullName = "Bob A.",
            Email = "existinguser@example.com",
            Phone = "+5511999999999",
            PasswordHash = "hash"
        });
        await RelationalContext.SaveChangesAsync();

        var registerUserRequest = new RegisterUserRequest
        {
            FullName = "Bob A.",
            Email = "existinguser@example.com",
            Phone = "+5511999999999",
            Password = "password123"
        };

        // Act
        var response = await _userService.Register(registerUserRequest);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("Email já está em uso.", response.Message);
    }

    /// <summary>
    /// Testa o registro de um usuário com campos inválidos.
    /// </summary>
    [Trait("Category", "Unit")]
    [Fact]
    public async Task Register_InvalidUserData_ReturnsError()
    {
        // Arrange: Criação de um request inválido (sem email ou senha)
        var registerUserRequest = new RegisterUserRequest
        {
            FullName = "Bob A.",
            Email = "", // Email vazio
            Phone = "+5511999999999",
            Password = "" // Senha vazia
        };

        // Act
        var response = await _userService.Register(registerUserRequest);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("Os dados do usuário são inválidos.", response.Message);
    }
}
