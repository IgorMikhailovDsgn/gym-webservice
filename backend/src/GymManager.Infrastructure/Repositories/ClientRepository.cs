using GymManager.Application.Abstractions;
using GymManager.Application.Clients;
using GymManager.Application.Common;
using GymManager.Infrastructure.Entities;
using GymManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Infrastructure.Repositories;

public sealed class ClientRepository : IClientRepository
{
    private readonly GymDbContext _db;

    public ClientRepository(GymDbContext db) => _db = db;

    public async Task<PagedResult<ClientModel>> GetPagedAsync(
        ClientQuery query,
        CancellationToken cancellationToken)
    {
        // IQueryable — ещё НЕ запрос к базе, а дерево выражений. Каждый Where
        // добавляет узел; SQL уйдёт только на CountAsync и ToListAsync.
        // Отложенное выполнение позволяет собирать фильтры по условиям,
        // не склеивая SQL строками.
        IQueryable<Client> clients = _db.Clients.AsNoTracking();

        if (query.Status is not null)
        {
            clients = clients.Where(c => c.Status == query.Status);
        }

        if (query.Search is not null)
        {
            var pattern = $"%{query.Search.ToLowerInvariant()}%";

            // Выражение слева ПОВТОРЯЕТ индекс ix_clients_fullname_trgm
            // символ в символ: lower(last_name || ' ' || first_name || ' '
            // || coalesce(middle_name, '')). Планировщик использует индекс по
            // выражению только при точном совпадении — отклонение переводит
            // запрос на последовательное сканирование.
            clients = clients.Where(c => EF.Functions.Like(
                (c.LastName + " " + c.FirstName + " " + (c.MiddleName ?? "")).ToLower(),
                pattern));
        }

        // Считаем ДО Skip/Take, но ПОСЛЕ фильтров: нужно общее число
        // подходящих записей, а не число на странице.
        var totalCount = await clients.CountAsync(cancellationToken);

        var items = await clients
            .OrderBy(c => c.LastName)
            // Второй ключ — гарантия устойчивой сортировки. Без него у двух
            // однофамильцев порядок между запросами не определён, и одна
            // запись может попасть и на первую, и на вторую страницу.
            .ThenBy(c => c.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            // Проекция выполняется в SQL: в SELECT попадут только эти колонки,
            // сущность Client не материализуется вовсе.
            .Select(c => new ClientModel(c.Id, c.LastName, c.FirstName, c.Phone, c.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientModel>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<ClientModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _db.Clients
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ClientModel(c.Id, c.LastName, c.FirstName, c.Phone, c.Status))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<ClientModel> AddAsync(
        CreateClientCommand command,
        CancellationToken cancellationToken)
    {
        // Id, Status, CreatedAt, UpdatedAt не задаём: их проставляет БД
        // через DEFAULT. EF знает это из скаффолда и не включает их в INSERT.
        var entity = new Client
        {
            LastName = command.LastName,
            FirstName = command.FirstName,
            MiddleName = command.MiddleName,
            Phone = command.Phone,
            Email = command.Email
        };

        // Add не пишет в базу — помещает объект в change tracker
        // со статусом Added.
        _db.Clients.Add(entity);

        // Вот здесь EF генерирует INSERT ... RETURNING id, status, created_at
        // и записывает сгенерированные значения обратно в entity.
        // Поэтому ниже entity.Id уже заполнен.
        await _db.SaveChangesAsync(cancellationToken);

        return new ClientModel(
            entity.Id,
            entity.LastName,
            entity.FirstName,
            entity.Phone,
            entity.Status);
    }

    public async Task<ClientModel?> UpdateAsync(
        Guid id,
        UpdateClientCommand command,
        CancellationToken cancellationToken)
    {
        // AsNoTracking здесь НЕТ намеренно. С отслеживанием EF запоминает
        // снимок исходных значений; на SaveChanges он сравнит его с текущим
        // состоянием и сгенерирует UPDATE только для изменившихся колонок.
        // С AsNoTracking SaveChanges тихо не сделал бы ничего.
        var entity = await _db.Clients
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
            return null;

        entity.LastName = command.LastName;
        entity.FirstName = command.FirstName;
        entity.MiddleName = command.MiddleName;
        entity.Phone = command.Phone;
        entity.Email = command.Email;
        entity.Status = command.Status;

        // _db.Clients.Update(entity) не нужен: сущность уже отслеживается.
        // Update пригодился бы для отсоединённой сущности, собранной вручную.
        await _db.SaveChangesAsync(cancellationToken);

        return new ClientModel(
            entity.Id,
            entity.LastName,
            entity.FirstName,
            entity.Phone,
            entity.Status);
    }
}
