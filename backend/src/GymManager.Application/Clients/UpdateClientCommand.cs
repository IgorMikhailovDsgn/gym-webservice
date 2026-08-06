namespace GymManager.Application.Clients;

/// <summary>
/// Данные для редактирования клиента.
/// Id не входит: он приходит из маршрута, иначе возможен конфликт
/// между адресом ресурса и телом запроса.
/// </summary>
public sealed record UpdateClientCommand(
    string LastName,
    string FirstName,
    string? MiddleName,
    string Phone,
    string? Email,
    string Status);
