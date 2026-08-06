using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymManager.Application.Abstractions;
using GymManager.Application.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GymManager.Infrastructure.Security;

public sealed class JwtTokenGenerator : ITokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration) => _configuration = configuration;

    public string Generate(UserCredentials user)
    {
        var section = _configuration.GetSection("Jwt");

        var key = section["Key"]
            ?? throw new InvalidOperationException("Не задан Jwt:Key.");

        // Claims — утверждения о пользователе внутри токена.
        // Sub (subject) — идентификатор; именно его мы будем доставать
        // при фиксации посещения вместо userId из тела запроса.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim("fullName", user.FullName),
            // Jti — уникальный идентификатор токена, нужен если понадобится
            // отзыв конкретного токена.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: section["Issuer"],
            audience: section["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(section["ExpiresMinutes"] ?? "480")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}