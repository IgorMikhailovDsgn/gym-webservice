using GymManager.Application.Abstractions;
using GymManager.Application.Common;

namespace GymManager.Application.Auth;

public interface IAuthService
{
    Task<AuthenticatedUser> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
}

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokens;

    public AuthService(IUserRepository users, IPasswordHasher hasher, ITokenGenerator tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthenticatedUser> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _users.FindByUsernameAsync(command.Username, cancellationToken);

        // ОДНО сообщение на все случаи: нет такого логина, неверный пароль,
        // учётка отключена. Разные тексты позволили бы перебором выяснить,
        // какие логины существуют.
        if (user is null || !user.IsActive || !_hasher.Verify(command.Password, user.PasswordHash))
            throw new BusinessRuleException("invalid_credentials", "Неверный логин или пароль.");

        return new AuthenticatedUser(
            user.Id,
            user.Username,
            user.FullName,
            _tokens.Generate(user));
    }
}