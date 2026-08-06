namespace GymManager.Application.Tickets;

/// <summary>
/// Абонемент с ВЫЧИСЛЕННЫМ статусом — читается из представления v_tickets.
///
/// VisitsLimit и VisitsRemaining допускают null: это соглашение из схемы,
/// означающее абонемент без ограничения по количеству посещений.
/// </summary>
public sealed record TicketModel(
    Guid Id,
    Guid ClientId,
    string TicketTypeName,
    DateOnly DateStart,
    DateOnly DateEnd,
    int? VisitsLimit,
    int VisitsUsed,
    int? VisitsRemaining,
    string Status);
