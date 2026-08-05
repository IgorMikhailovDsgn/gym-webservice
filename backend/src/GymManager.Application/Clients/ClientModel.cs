namespace GymManager.Application.Clients;

public sealed record ClientModel(
    Guid Id,
    string LastName,
    string FirstName,
    string Phone,
    string Status);