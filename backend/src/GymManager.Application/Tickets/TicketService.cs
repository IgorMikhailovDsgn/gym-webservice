using GymManager.Application.Abstractions;
using GymManager.Application.Common;

namespace GymManager.Application.Tickets;

public interface ITicketService
{
    Task<IReadOnlyList<TicketModel>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken);
    Task<TicketModel> CreateAsync(CreateTicketCommand command, CancellationToken cancellationToken);
    Task<TicketModel> ExtendAsync(Guid ticketId, ExtendTicketCommand command, CancellationToken cancellationToken);
    Task<PagedResult<TicketModel>> SearchAsync(TicketQuery query, CancellationToken cancellationToken);
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

    public async Task<TicketModel> CreateAsync(
    CreateTicketCommand command,
    CancellationToken cancellationToken)
    {
        var client = await _clients.GetByIdAsync(command.ClientId, cancellationToken);

        if (client is null)
            throw NotFoundException.For("Клиент", command.ClientId);

        if (client.Status == "blocked")
            throw new BusinessRuleException(
                "client_blocked", "Клиент заблокирован, оформление абонемента невозможно.");

        var type = await _tickets.GetTypeAsync(command.TicketTypeId, cancellationToken);

        if (type is null)
            throw NotFoundException.For("Тип абонемента", command.TicketTypeId);

        // Условия копируются из шаблона в момент продажи — это снимок,
        // а не ссылка. DurationDays может быть null: тогда срок нужно
        // задавать вручную, чего мы пока не поддерживаем.
        if (type.DurationDays is null)
            throw new BusinessRuleException(
                "duration_required",
                $"Для типа «{type.Name}» срок действия задаётся вручную.");

        // DateEnd включительно, поэтому минус один день:
        // месячный с 1-го числа действует по 30-е, а не по 31-е.
        var dateEnd = command.DateStart.AddDays(type.DurationDays.Value - 1);

        return await _tickets.AddAsync(
            command.ClientId, type.Id, command.DateStart, dateEnd,
            type.DefaultVisits, cancellationToken);
    }

    public async Task<TicketModel> ExtendAsync(
        Guid ticketId,
        ExtendTicketCommand command,
        CancellationToken cancellationToken)
    {
        var state = await _tickets.GetStateAsync(ticketId, cancellationToken);

        if (state is null)
            throw NotFoundException.For("Абонемент", ticketId);

        if (state.IsCancelled)
            throw new BusinessRuleException(
                "ticket_cancelled", "Отменённый абонемент продлить нельзя.");

        return await _tickets.ExtendAsync(ticketId, command.Days, cancellationToken);
    }

    public Task<PagedResult<TicketModel>> SearchAsync(
    TicketQuery query,
    CancellationToken cancellationToken)
    {
        var normalized = query with
        {
            Page = query.Page < 1 ? 1 : query.Page,
            PageSize = Math.Clamp(query.PageSize, 1, 100),
            Status = string.IsNullOrWhiteSpace(query.Status) ? null : query.Status.Trim().ToLowerInvariant()
        };

        return _tickets.SearchAsync(normalized, cancellationToken);
    }
    
}
