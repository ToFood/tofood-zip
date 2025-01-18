using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToFood.Domain.DB.NonRelational;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Entities.Relational;

namespace ToFood.Domain.Services;

/// <summary>
/// Serviço responsável por autenticação, registro de usuários e geração de tokens JWT.
/// </summary>
public class AuthService
{
    /// <summary>
    /// Conexão com o banco relacional
    /// </summary>
    private readonly ToFoodRelationalContext _dbRelationalContext;
    private readonly ToFoodNonRelationalContext _dbNonRelationalContext;
    private readonly string _jwtSecret = "MySuperSecureAndLongerKeywithsize128123456"; // Chave secreta do token

    /// <summary>
    /// Inicializa uma nova instância do serviço de autenticação com o contexto do banco de dados.
    /// </summary>
    /// <param name="dbRelationalContext">O contexto do banco de dados.</param>
    public AuthService(ToFoodRelationalContext dbRelationalContext, ToFoodNonRelationalContext toFoodNonRelationalContext)
    {
        _dbRelationalContext = dbRelationalContext;
        _dbNonRelationalContext = toFoodNonRelationalContext;
    }

    /// <summary>
    /// Registra um novo usuário no sistema.
    /// </summary>
    /// <param name="email">O email do usuário.</param>
    /// <param name="password">A senha do usuário.</param>
    /// <returns>Um objeto contendo o resultado da operação.</returns>
    public async Task<AuthResponse> Register(string email, string password)
    {
        if (await _dbRelationalContext.Users.AnyAsync(u => u.Email == email))
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Email já está em uso."
            };
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash
        };

        _dbRelationalContext.Users.Add(user);
        await _dbRelationalContext.SaveChangesAsync();

        return new AuthResponse
        {
            IsSuccess = true,
            Message = "Usuário registrado com sucesso!"
        };
    }

    /// <summary>
    /// Realiza o login de um usuário, verificando as credenciais fornecidas e retornando um token JWT.
    /// </summary>
    /// <param name="email">O email do usuário.</param>
    /// <param name="password">A senha do usuário.</param>
    /// <returns>Um objeto contendo o resultado da operação e o token JWT, se bem-sucedido.</returns>
    public async Task<AuthResponse> Login(string email, string password)
    {
        var user = await _dbRelationalContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "E-mail ou senha inválidos."
            };
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            IsSuccess = true,
            Message = "Login bem-sucedido!",
            Token = token
        };
    }

    /// <summary>
    /// Retorna uma lista de usuários registrados no sistema.
    /// </summary>
    /// <returns>Uma lista de usuários com seus IDs, emails e datas de criação.</returns>
    public async Task<object> GetUsers()
    {
        return await _dbRelationalContext.Users
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.CreatedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Gera um token JWT para autenticar o usuário.
    /// </summary>
    /// <param name="user">O usuário para o qual o token será gerado.</param>
    /// <returns>Um token JWT como string.</returns>
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user?.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user?.Id.ToString() ?? "")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "your-issuer",
            Audience = "your-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

/// <summary>
/// Representa a resposta das operações de autenticação, incluindo mensagens e tokens.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Mensagem de retorno da operação.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Token JWT gerado, se aplicável.
    /// </summary>
    public string? Token { get; set; }
}
