namespace GymManager.Application.Clients;

/// <summary>
/// Параметры выборки клиентов. Отдельный тип вместо набора аргументов:
/// добавление сортировки не сломает сигнатуру у вызывающих.
/// </summary>
public sealed record ClientQuery
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
