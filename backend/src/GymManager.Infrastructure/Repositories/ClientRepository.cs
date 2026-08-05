using GymManager.Application.Abstractions;
using GymManager.Application.Clients;
using GymManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using GymManager.Application.Common;
using GymManager.Infrastructure.Entities;

namespace GymManager.Infrastructure.Repositories;

public sealed class ClientRepository : IClientRepository
{
    private readonly GymDbContext _db;

    public ClientRepository(GymDbContext db) => _db = db;

    public async Task<PagedResult<ClientModel>> GetPagedAsync(
    ClientQuery query,
    CancellationToken cancellationToken)
    {
        IQueryable<Client> clients = _db.Clients.AsNoTracking();

        var totalCount = await clients.CountAsync(cancellationToken);

        var items = await clients
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new ClientModel(c.Id, c.LastName, c.FirstName, c.Phone, c.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientModel>(items, query.Page, query.PageSize, totalCount);
    }
}