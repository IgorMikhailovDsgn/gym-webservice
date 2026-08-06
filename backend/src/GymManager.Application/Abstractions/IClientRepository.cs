using GymManager.Application.Clients;
using GymManager.Application.Common;

namespace GymManager.Application.Abstractions;

/// <summary>
/// Контракт доступа к данным о клиентах.
///
/// Интерфейс объявлен в Application, а реализован в Infrastructure — это
/// инверсия зависимостей: интерфейс принадлежит тому, кто им пользуется.
/// Благодаря этому Application не ссылается ни на EF, ни на Npgsql.
///
/// Методы возвращают модели прикладного слоя, а не сущности EF:
/// тип хранилища за границу интерфейса не проникает.
/// </summary>
public interface IClientRepository
{
    Task<PagedResult<ClientModel>> GetPagedAsync(ClientQuery query, CancellationToken cancellationToken);

    /// Возвращает null, если клиента нет — решение принимает сервис.
    Task<ClientModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ClientModel> AddAsync(CreateClientCommand command, CancellationToken cancellationToken);

    Task<ClientModel?> UpdateAsync(Guid id, UpdateClientCommand command, CancellationToken cancellationToken);
}
