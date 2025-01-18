using Microsoft.EntityFrameworkCore;
using Moq;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Services;

namespace ToFood.Tests.UnitTests;

/// <summary>
/// Testes unitários para o serviço de autenticação (AuthService).
/// </summary>
public class AuthServiceUnitTests
{
    private readonly Mock<ToFoodRelationalContext> _mockContext;
    private readonly Mock<DbSet<User>> _mockUserDbSet;
    private readonly AuthService _authService;

    public AuthServiceUnitTests()
    {
        // Mock do DbSet<User>
        _mockUserDbSet = new Mock<DbSet<User>>();

        // Mock do DbContext
        _mockContext = new Mock<ToFoodRelationalContext>();
        _mockContext.Setup(c => c.Users).Returns(_mockUserDbSet.Object);

        // Mock do IConfiguration
        var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("MySuperSecureAndLongerKeywithsize128123456");
        mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("your-issuer");
        mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("your-audience");

        // Instância do AuthService com os mocks
        _authService = new AuthService(_mockContext.Object);
    }

    /// <summary>
    /// Testa o registro de um novo usuário com sucesso.
    /// </summary>
    [Fact]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Arrange
        _mockUserDbSet.Setup(m => m.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), default))
            .ReturnsAsync(false); // Nenhum usuário com o mesmo email

        // Act
        var response = await _authService.Register("newuser@example.com", "password123");

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal("Usuário registrado com sucesso!", response.Message);

        _mockUserDbSet.Verify(m => m.Add(It.IsAny<User>()), Times.Once); // Verifica se o Add foi chamado
        _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once); // Verifica se o SaveChanges foi chamado
    }

    /// <summary>
    /// Testa o registro de um usuário com email já existente.
    /// </summary>
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsError()
    {
        // Arrange
        _mockUserDbSet.Setup(m => m.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), default))
            .ReturnsAsync(true); // Simula um usuário já existente

        // Act
        var response = await _authService.Register("existinguser@example.com", "password123");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("Email já está em uso.", response.Message);

        _mockUserDbSet.Verify(m => m.Add(It.IsAny<User>()), Times.Never); // Verifica que o Add não foi chamado
        _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Never); // Verifica que o SaveChanges não foi chamado
    }

    /// <summary>
    /// Testa o login de um usuário com credenciais válidas.
    /// </summary>
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = new User { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") };
        _mockUserDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(user); // Simula um usuário encontrado

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
        _mockUserDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
        .ReturnsAsync((User?)null); // Simula nenhum usuário encontrado


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
        var user = new User { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") };
        _mockUserDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), default))
            .ReturnsAsync(user); // Simula um usuário encontrado

        // Act
        var response = await _authService.Login("test@example.com", "wrongpassword");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("E-mail ou senha inválidos.", response.Message);
    }
}
