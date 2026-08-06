namespace GymManager.Application.Tickets;

/// <summary>
/// Параметры выборки абонементов.
///
/// Status фильтрует по ВЫЧИСЛЯЕМОМУ статусу из v_tickets. Это возможно только
/// потому, что статус считается в БД: будь он в C#, пришлось бы вытащить все
/// абонементы в память и фильтровать там.
/// </summary>
public sealed record TicketQuery
{
    public Guid? ClientId { get; init; }
    public string? Status { get; init; }

    /// <summary>Абонементы, действующие на эту дату.</summary>
    public DateOnly? ActiveOn { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}