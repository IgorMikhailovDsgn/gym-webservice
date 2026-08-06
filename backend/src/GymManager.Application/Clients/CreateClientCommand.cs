namespace GymManager.Application.Clients;

/// <summary>
/// Данные для создания клиента.
/// Id, Status и CreatedAt отсутствуют намеренно — их проставляет БД.
/// </summary>
public sealed record CreateClientCommand(
    string LastName,
    string FirstName,
    string? MiddleName,
    string Phone,
    string? Email);
