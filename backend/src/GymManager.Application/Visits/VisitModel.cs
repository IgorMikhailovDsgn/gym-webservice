namespace GymManager.Application.Visits;

/// Команда на фиксацию посещения.
public sealed record RegisterVisitCommand(
    Guid TicketId,
    Guid? TrainerId,
    Guid UserId);

/// Зафиксированное посещение.
public sealed record VisitModel(
    Guid Id,
    Guid TicketId,
    DateTime VisitedAt,
    Guid? TrainerId,
    Guid UserId,
    int? VisitsRemaining);

/// Снимок абонемента для проверки правил.
/// Отдельный тип от TicketModel: там статус УЖЕ вычислен представлением,
/// а здесь нужны сырые поля, чтобы проверить каждое правило отдельно
/// и вернуть точную причину отказа.
public sealed record TicketState(
    Guid Id,
    Guid ClientId,
    DateOnly DateStart,
    DateOnly DateEnd,
    int? VisitsLimit,
    int VisitsUsed,
    bool IsCancelled);