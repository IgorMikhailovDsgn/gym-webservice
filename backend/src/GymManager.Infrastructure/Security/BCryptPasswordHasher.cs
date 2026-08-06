using GymManager.Application.Abstractions;

namespace GymManager.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    // Cost 11 — тот же, что использован при генерации хешей в сиде.
    private const int WorkFactor = 11;

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
}