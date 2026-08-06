using GymManager.Application.Abstractions;
using GymManager.Application.Auth;
using GymManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly GymDbContext _db;

    public UserRepository(GymDbContext db) => _db = db;

    public Task<UserCredentials?> FindByUsernameAsync(
        string username,
        CancellationToken cancellationToken)
        => _db.Users
            .AsNoTracking()
            .Where(u => u.Username == username)
            .Select(u => new UserCredentials(
                u.Id,
                u.Username,
                u.PasswordHash,
                u.LastName + " " + u.FirstName,
                u.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
}