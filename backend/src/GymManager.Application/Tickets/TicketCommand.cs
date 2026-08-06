namespace GymManager.Application.Tickets;

/// Оформление абонемента.
///
/// DateEnd и VisitsLimit не задаются: они вычисляются из шаблона
/// ticket_types и КОПИРУЮТСЯ в абонемент. Если бы абонемент ссылался
/// на справочник, правка «месячный теперь 10 занятий вместо 8» задним
/// числом изменила бы условия всех проданных абонементов.
public sealed record CreateTicketCommand(
    Guid ClientId,
    Guid TicketTypeId,
    DateOnly DateStart);

/// Продление абонемента на указанное число дней.
public sealed record ExtendTicketCommand(int Days);

/// Тип абонемента — шаблон условий.
public sealed record TicketTypeModel(
    Guid Id,
    string Code,
    string Name,
    int? DurationDays,
    int? DefaultVisits);