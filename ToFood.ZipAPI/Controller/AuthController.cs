using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToFood.Domain.Services;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    /// <summary>
    /// Inicializa uma nova instância do AuthController com o serviço de autenticação.
    /// </summary>
    /// <param name="authService">O serviço responsável por operações de autenticação.</param>
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Endpoint para autenticar um usuário e gerar um token JWT.
    /// </summary>
    /// <param name="email">O email do usuário.</param>
    /// <param name="password">A senha do usuário.</param>
    /// <returns>Uma resposta com um token JWT se as credenciais forem válidas.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var response = await _authService.Login(email, password);

        if (!response.IsSuccess)
            return Unauthorized(response.Message); // Retorna um erro de autorização se as credenciais forem inválidas

        return Ok(new
        {
            response.Message, // Mensagem de sucesso
            response.Token    // Token JWT gerado
        });
    }
}
