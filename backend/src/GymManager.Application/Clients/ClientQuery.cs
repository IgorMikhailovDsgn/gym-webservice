namespace GymManager.Application.Clients;

public sealed record ClientQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}