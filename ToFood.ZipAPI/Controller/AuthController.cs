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
    /// Endpoint para registrar um novo usuário.
    /// </summary>
    /// <param name="email">O email do usuário a ser registrado.</param>
    /// <param name="password">A senha do usuário a ser registrado.</param>
    /// <returns>Uma resposta indicando o sucesso ou falha do registro.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(string email, string password)
    {
        var response = await _authService.Register(email, password);

        if (!response.IsSuccess)
            return BadRequest(response.Message); // Retorna um erro se o registro falhar

        return Ok(response.Message); // Retorna uma mensagem de sucesso
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

    /// <summary>
    /// Endpoint para obter uma lista de todos os usuários registrados.
    /// Requer autenticação.
    /// </summary>
    /// <returns>Uma lista de usuários com informações básicas.</returns>
    [Authorize]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _authService.GetUsers(); // Obtém os usuários do serviço
        return Ok(users); // Retorna a lista de usuários
    }
}
