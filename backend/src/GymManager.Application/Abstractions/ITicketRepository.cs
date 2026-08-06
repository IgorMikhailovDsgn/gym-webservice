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
}