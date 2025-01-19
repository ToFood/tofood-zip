using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ToFood.Domain.Services;

[ApiController]
[Route("User")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    /// <summary>
    /// Inicializa uma nova instância do UserController com o serviço de autenticação.
    /// </summary>
    /// <param name="UserService">O serviço responsável por operações de autenticação.</param>
    public UserController(UserService UserService)
    {
        _userService = UserService;
    }

    /// <summary>
    /// Endpoint para registrar um novo usuário.
    /// </summary>
    /// <param name="email">O email do usuário a ser registrado.</param>
    /// <param name="password">A senha do usuário a ser registrado.</param>
    /// <returns>Uma resposta indicando o sucesso ou falha do registro.</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(string email, string password)
    {
        var response = await _userService.Register(email, password);

        if (!response.IsSuccess)
            return BadRequest(response.Message); // Retorna um erro se o registro falhar

        return Ok(response.Message); // Retorna uma mensagem de sucesso
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
        var users = await _userService.GetUsers(); // Obtém os usuários do serviço
        return Ok(users); // Retorna a lista de usuários
    }
}
