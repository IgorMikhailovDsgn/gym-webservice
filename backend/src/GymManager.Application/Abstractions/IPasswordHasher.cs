namespace GymManager.Application.Abstractions;

/// Проверка пароля вынесена за интерфейс, потому что BCrypt — деталь
/// реализации. Захочется перейти на Argon2 — меняется одна реализация.
public interface IPasswordHasher
{
    bool Verify(string password, string hash);
    string Hash(string password);
}