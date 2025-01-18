using Microsoft.EntityFrameworkCore;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Services;

namespace ToFood.Tests.IntegrationTests;

/// <summary>
/// Classe de testes para o serviço de autenticação (AuthService).
/// </summary>
public class AuthIntegrationTests : TestBase
{
    private readonly AuthService _authService;

    /// <summary>
    /// Construtor que inicializa a classe de testes.
    /// </summary>
    public AuthIntegrationTests()
    {
        // Inicializa o AuthService com o contexto e a configuração fornecidos pela TestBase
        _authService = new AuthService(RelationalContext);
    }

    /// <summary>
    /// Testa o registro de um novo usuário com sucesso.
    /// </summary>
    [Fact]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Act
        var response = await _authService.Register("newuser@example.com", "password123");

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal("Usuário registrado com sucesso!", response.Message);

        var user = await RelationalContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        Assert.NotNull(user);
    }

    /// <summary>
    /// Testa o registro de um usuário já existente (falha esperada).
    /// </summary>
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsError()
    {
        // Act
        var response = await _authService.Register("test@example.com", "password123");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("Email já está em uso.", response.Message);
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
    /// Testa a recuperação da lista de usuários.
    /// </summary>
    [Fact]
    public async Task GetUsers_ReturnsUserList()
    {
        // Act
        var users = await _authService.GetUsers();

        // Assert
        Assert.NotNull(users);

        // Converte o retorno para IEnumerable<object> de forma explícita
        var userList = users as IEnumerable<object>;
        Assert.NotNull(userList); // Garante que a conversão foi bem-sucedida
        Assert.NotEmpty(userList); // Verifica que a coleção não está vazia
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
}
