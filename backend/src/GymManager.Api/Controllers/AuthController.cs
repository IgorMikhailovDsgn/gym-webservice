using GymManager.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers;

/// <summary>Аутентификация сотрудников.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Вход в систему. Возвращает JWT для последующих запросов.</summary>
    /// <remarks>
    /// Полученный токен передаётся в заголовке: Authorization: Bearer {token}
    /// </remarks>
    [HttpPost("login")]
    // Формально избыточен: на классе нет [Authorize]. Оставлен как явное
    // заявление о намерении — если завтра [Authorize] повесят на весь
    // контроллер, вход не сломается.
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _auth.LoginAsync(command, cancellationToken);
        return Ok(user);
    }
}
