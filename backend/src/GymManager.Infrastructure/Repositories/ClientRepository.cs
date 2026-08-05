using GymManager.Application.Abstractions;
using GymManager.Application.Clients;
using GymManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Infrastructure.Repositories;

public sealed class ClientRepository : IClientRepository
{
    private readonly GymDbContext _db;

    public ClientRepository(GymDbContext db) => _db = db;

    public async Task<IReadOnlyList<ClientModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _db.Clients
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .Select(c => new ClientModel(c.Id, c.LastName, c.FirstName, c.Phone, c.Status))
            .ToListAsync(cancellationToken);
    }
}