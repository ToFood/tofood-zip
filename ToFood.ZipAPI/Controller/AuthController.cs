using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return BadRequest("Email já está em uso.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Usuário registrado com sucesso!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        // Verificar se o e-mail existe
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return Unauthorized("E-mail ou senha inválidos.");

        // Validar a senha fornecida com o hash armazenado
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Unauthorized("E-mail ou senha inválidos.");

        // (Opcional) Gerar um token JWT para o usuário logado
        var token = GenerateJwtToken(user);

        return Ok(new
        {
            Message = "Login bem-sucedido!",
            Token = token
        });
    }

    // Método para gerar um token JWT (opcional)
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("MySuperSecureAndLongerKeywithsize128123456"); // Use uma chave segura

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        }),
            Expires = DateTime.UtcNow.AddHours(1), // Expiração do token
            Issuer = "your-issuer",
            Audience = "your-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Authorize]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        // Obter todos os usuários do banco de dados
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }


}


