namespace GymManager.Application.Common;

/// <summary>
/// Запрошенный объект не существует. Middleware превращает в 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public static NotFoundException For(string entity, Guid id)
        => new($"{entity} с идентификатором {id} не найден.");
}
