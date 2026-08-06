using GymManager.Application.Tickets;
using GymManager.Application.Visits;

namespace GymManager.Application.Abstractions;

public interface ITicketRepository
{
    Task<IReadOnlyList<TicketModel>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken);

    /// Читает абонемент, БЛОКИРУЯ строку до конца транзакции.
    /// Вызывать только внутри ExecuteInTransactionAsync.
    Task<TicketState?> GetForUpdateAsync(Guid ticketId, CancellationToken cancellationToken);

    /// Создаёт посещение и увеличивает счётчик использованных.
    Task<VisitModel> AddVisitAsync(
        Guid ticketId, Guid? trainerId, Guid userId, CancellationToken cancellationToken);

    Task<TicketTypeModel?> GetTypeAsync(Guid ticketTypeId, CancellationToken cancellationToken);

    Task<TicketModel> AddAsync(
    Guid clientId, Guid ticketTypeId, DateOnly dateStart, DateOnly dateEnd,
    int? visitsLimit, CancellationToken cancellationToken);

    Task<TicketState?> GetStateAsync(Guid ticketId, CancellationToken cancellationToken);

    Task<TicketModel> ExtendAsync(Guid ticketId, int days, CancellationToken cancellationToken);
}