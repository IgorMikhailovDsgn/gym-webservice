namespace GymManager.Application.Auth;

public sealed record LoginCommand(string Username, string Password);

public sealed record AuthenticatedUser(
    Guid Id,
    string Username,
    string FullName,
    string Token);

/// Данные пользователя для проверки пароля.
/// Хеш наружу из Application не выходит — только сюда.
public sealed record UserCredentials(
    Guid Id,
    string Username,
    string PasswordHash,
    string FullName,
    bool IsActive);