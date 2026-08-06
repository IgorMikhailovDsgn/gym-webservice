using GymManager.Application.Abstractions;
using GymManager.Application.Common;

namespace GymManager.Application.Tickets;

public interface ITicketService
{
    Task<IReadOnlyList<TicketModel>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken);
}

public sealed class TicketService : ITicketService
{
    private readonly ITicketRepository _tickets;
    private readonly IClientRepository _clients;

    // Несколько репозиториев в одном сервисе — норма: сервис координирует
    // работу с разными агрегатами.
    public TicketService(ITicketRepository tickets, IClientRepository clients)
    {
        _tickets = tickets;
        _clients = clients;
    }

    public async Task<IReadOnlyList<TicketModel>> GetByClientAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        // Проверка нужна, чтобы различить два разных состояния:
        // «клиента нет» (ошибка запроса, 404) и «у клиента нет
        // абонементов» (валидное состояние, пустой массив).
        var client = await _clients.GetByIdAsync(clientId, cancellationToken);

        if (client is null)
            throw NotFoundException.For("Клиент", clientId);

        return await _tickets.GetByClientAsync(clientId, cancellationToken);
    }
}
