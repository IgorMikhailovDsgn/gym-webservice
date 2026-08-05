using GymManager.Application.Clients;
using GymManager.Application.Common;

namespace GymManager.Application.Abstractions;

public interface IClientRepository
{
    Task<PagedResult<ClientModel>> GetPagedAsync(ClientQuery query, CancellationToken cancellationToken);
}