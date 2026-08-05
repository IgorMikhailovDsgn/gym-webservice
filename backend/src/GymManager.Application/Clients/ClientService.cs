using GymManager.Application.Abstractions;
using GymManager.Application.Common;

namespace GymManager.Application.Clients;

public interface IClientService
{
    Task<PagedResult<ClientModel>> GetPagedAsync(ClientQuery query, CancellationToken cancellationToken);
}

public sealed class ClientService : IClientService
{
    private const int MaxPageSize = 100;

    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository) => _repository = repository;

    public Task<PagedResult<ClientModel>> GetPagedAsync(
        ClientQuery query,
        CancellationToken cancellationToken)
    {
        var normalized = query with
        {
            Page = query.Page < 1 ? 1 : query.Page,
            PageSize = Math.Clamp(query.PageSize, 1, MaxPageSize)
        };

        return _repository.GetPagedAsync(normalized, cancellationToken);
    }
}