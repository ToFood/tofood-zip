﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToFood.Domain.DB.Relational;
using ToFood.Domain.Entities.NonRelational;
using ToFood.Domain.Entities.Relational;
using ToFood.Domain.Helpers;

namespace ToFood.Domain.Services;

/// <summary>
/// Serviço responsável por autenticação, registro de usuários e geração de tokens JWT.
/// </summary>
public class AuthService
{
    private readonly ToFoodRelationalContext _dbRelationalContext;
    private readonly ILogger<AuthService> _logger;
    private readonly string _jwtSecret = "MySuperSecureAndLongerKeywithsize128123456"; // Chave secreta do token

    /// <summary>
    /// Inicializa uma nova instância do serviço de autenticação com o contexto do banco de dados e LogHelper.
    /// </summary>
    public AuthService(
        ToFoodRelationalContext dbRelationalContext,
        ILogger<AuthService> logger
        )
    {
        _dbRelationalContext = dbRelationalContext;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo usuário no sistema.
    /// <param name="email">O email do usuário.</param>
    /// <param name="password">A senha do usuário.</param>
    /// <returns>Um objeto contendo o resultado da operação.</returns>
    /// </summary>
    public async Task<AuthResponse> Register(string email, string password)
    {
        _logger.LogInformation("Exemplo de log no MongoDB.");

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

        try
        {
            _dbRelationalContext.Users.Add(user);
            await _dbRelationalContext.SaveChangesAsync();

            _logger.LogInformation("Exemplo de log no MongoDB.");

            return new AuthResponse
            {
                IsSuccess = true,
                Message = "Usuário registrado com sucesso!"
            };
        }
        catch (Exception)
        {
            _logger.LogInformation("Exemplo de log no MongoDB.");

            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Erro interno no servidor."
            };
        }
    }

    /// <summary>
    /// Realiza o login de um usuário, verificando as credenciais fornecidas e retornando um token JWT.
    /// </summary>
    public async Task<AuthResponse> Login(string email, string password)
    {
        _logger.LogInformation("Exemplo de log no MongoDB.");

        var user = await _dbRelationalContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogInformation("Exemplo de log no MongoDB.");

            return new AuthResponse
            {
                IsSuccess = false,
                Message = "E-mail ou senha inválidos."
            };
        }

        var token = GenerateJwtToken(user);

        _logger.LogInformation("Exemplo de log no MongoDB.");

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
