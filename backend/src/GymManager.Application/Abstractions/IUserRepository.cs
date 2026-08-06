using GymManager.Application.Auth;

namespace GymManager.Application.Abstractions;

public interface IUserRepository
{
    Task<UserCredentials?> FindByUsernameAsync(string username, CancellationToken cancellationToken);
}