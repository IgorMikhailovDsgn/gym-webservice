using GymManager.Application.Abstractions;

namespace GymManager.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly GymDbContext _db;

    public UnitOfWork(GymDbContext db) => _db = db;

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await action(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}