namespace GymManager.Application.Clients;

/// <summary>
/// Клиент с точки зрения прикладного слоя.
/// НЕ сущность EF: сущности живут в Infrastructure и наружу не выходят.
/// </summary>
public sealed record ClientModel(
    Guid Id,
    string LastName,
    string FirstName,
    string Phone,
    string Status);
