using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymManager.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// Достаёт идентификатор пользователя из claim Sub.
    /// ASP.NET Core по умолчанию переименовывает Sub в ClaimTypes.NameIdentifier,
    /// поэтому проверяем оба варианта.
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("В токене отсутствует идентификатор пользователя.");
    }
}