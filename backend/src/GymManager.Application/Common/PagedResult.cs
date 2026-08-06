namespace GymManager.Application.Common;

/// <summary>
/// Страница результатов с метаданными навигации.
/// Обобщённый тип: одна реализация обслуживает клиентов, абонементы, посещения.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    // Вычисляется, а не хранится: полностью выводится из TotalCount и PageSize,
    // поэтому отдельное поле создало бы второй источник правды.
    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
