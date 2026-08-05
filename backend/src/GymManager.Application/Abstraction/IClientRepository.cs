using GymManager.Application.Clients;

namespace GymManager.Application.Abstractions;

public interface IClientRepository
{
    Task<IReadOnlyList<ClientModel>> GetAllAsync(CancellationToken cancellationToken);
}