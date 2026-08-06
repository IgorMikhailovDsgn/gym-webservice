using GymManager.Application.Abstractions;
using GymManager.Application.Common;

namespace GymManager.Application.Clients;

public interface IClientService
{
    Task<PagedResult<ClientModel>> GetPagedAsync(ClientQuery query, CancellationToken cancellationToken);
    Task<ClientModel> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ClientModel> CreateAsync(CreateClientCommand command, CancellationToken cancellationToken);
    Task<ClientModel> UpdateAsync(Guid id, UpdateClientCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Прикладной сервис: правила, не зависящие ни от способа хранения, ни от HTTP.
/// </summary>
public sealed class ClientService : IClientService
{
    // Потолок размера страницы. Без него ?pageSize=1000000 вытащил бы
    // всю таблицу в память. Это бизнес-правило, поэтому живёт здесь,
    // а не в контроллере (обошли бы из фонового задания) и не в
    // репозитории (он должен получать уже корректный запрос).
    private const int MaxPageSize = 100;

    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository) => _repository = repository;

    public Task<PagedResult<ClientModel>> GetPagedAsync(
        ClientQuery query,
        CancellationToken cancellationToken)
    {
        var normalized = query with
        {
            Page = query.Page < 1 ? 1 : query.Page,
            PageSize = Math.Clamp(query.PageSize, 1, MaxPageSize),
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            Status = string.IsNullOrWhiteSpace(query.Status) ? null : query.Status.Trim().ToLowerInvariant()
        };

        // async/await не нужны: после вызова делать нечего, отдаём чужую Task.
        // Лишняя пара добавила бы конечный автомат без пользы.
        return _repository.GetPagedAsync(normalized, cancellationToken);
    }

    public async Task<ClientModel> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken);

        // Репозиторий сообщает факт «не найдено» через null.
        // Решение «это ошибка» принимает сервис.
        return client ?? throw NotFoundException.For("Клиент", id);
    }

    public Task<ClientModel> CreateAsync(CreateClientCommand command, CancellationToken cancellationToken)
    {
        var normalized = command with
        {
            LastName = command.LastName.Trim(),
            FirstName = command.FirstName.Trim(),
            MiddleName = Normalize(command.MiddleName),
            Phone = command.Phone.Trim(),
            Email = Normalize(command.Email)
        };

        return _repository.AddAsync(normalized, cancellationToken);
    }

    public async Task<ClientModel> UpdateAsync(
        Guid id,
        UpdateClientCommand command,
        CancellationToken cancellationToken)
    {
        var normalized = command with
        {
            LastName = command.LastName.Trim(),
            FirstName = command.FirstName.Trim(),
            MiddleName = Normalize(command.MiddleName),
            Phone = command.Phone.Trim(),
            Email = Normalize(command.Email),
            Status = command.Status.Trim().ToLowerInvariant()
        };

        var client = await _repository.UpdateAsync(id, normalized, cancellationToken);

        return client ?? throw NotFoundException.For("Клиент", id);
    }

    // Пустая строка и строка из пробелов приводятся к null.
    // В БД middle_name и email объявлены NULL-able именно как «значения нет»;
    // пустая строка — третье состояние, ломающее поиск и отображение.
    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
