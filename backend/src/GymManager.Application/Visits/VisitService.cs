using GymManager.Application.Abstractions;
using GymManager.Application.Common;

namespace GymManager.Application.Visits;

public interface IVisitService
{
    Task<VisitModel> RegisterAsync(RegisterVisitCommand command, CancellationToken cancellationToken);
}

public sealed class VisitService : IVisitService
{
    private readonly ITicketRepository _tickets;
    private readonly IUnitOfWork _unitOfWork;

    public VisitService(ITicketRepository tickets, IUnitOfWork unitOfWork)
    {
        _tickets = tickets;
        _unitOfWork = unitOfWork;
    }

    public Task<VisitModel> RegisterAsync(
        RegisterVisitCommand command,
        CancellationToken cancellationToken)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Блокировка строки: до конца транзакции второй запрос по этому
            // же абонементу будет ждать здесь. Без неё два быстрых клика
            // прошли бы проверку лимита одновременно и списали два посещения
            // при одном оставшемся.
            var ticket = await _tickets.GetForUpdateAsync(command.TicketId, ct);

            if (ticket is null)
                throw NotFoundException.For("Абонемент", command.TicketId);

            EnsureCanRegisterVisit(ticket);

            return await _tickets.AddVisitAsync(
                ticket.Id, command.TrainerId, command.UserId, ct);
        }, cancellationToken);
    }

    /// Четыре бизнес-правила из ТЗ. Порядок проверок = порядок приоритета
    /// причин отказа: отменённый абонемент остаётся отменённым, даже если
    /// у него ещё и срок вышел.
    private static void EnsureCanRegisterVisit(TicketState ticket)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (ticket.IsCancelled)
            throw new BusinessRuleException(
                "ticket_cancelled", "Абонемент отменён.");

        if (today < ticket.DateStart)
            throw new BusinessRuleException(
                "ticket_not_started",
                $"Абонемент начинает действовать {ticket.DateStart:dd.MM.yyyy}.");

        if (today > ticket.DateEnd)
            throw new BusinessRuleException(
                "ticket_expired",
                $"Срок действия абонемента истёк {ticket.DateEnd:dd.MM.yyyy}.");

        if (ticket.VisitsLimit is not null && ticket.VisitsUsed >= ticket.VisitsLimit)
            throw new BusinessRuleException(
                "ticket_exhausted",
                $"Посещения исчерпаны: использовано {ticket.VisitsUsed} из {ticket.VisitsLimit}.");
    }
}