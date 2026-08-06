using GymManager.Application.Tickets;

namespace GymManager.Application.Abstractions;

public interface ITicketRepository
{
    Task<IReadOnlyList<TicketModel>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken);
}
