namespace GymManager.Application.Abstractions;


/// Граница транзакции.
/// Нужна, потому что фиксация посещения — это ДВЕ записи (строка в visits
/// и инкремент visits_used), которые обязаны примениться вместе. При этом
/// проверка правил происходит между ними и должна попасть в ту же транзакцию.
/// Интерфейс отдаёт делегат, а не объект транзакции: так Application
/// не узнаёт про IDbContextTransaction из EF, а коммит и откат гарантированно
/// не забудут вызвать.
public interface IUnitOfWork
{
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken);
}