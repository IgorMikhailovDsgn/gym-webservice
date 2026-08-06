using GymManager.Application.Abstractions;
using GymManager.Application.Tickets;
using GymManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Infrastructure.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly GymDbContext _db;

    public TicketRepository(GymDbContext db) => _db = db;

    public async Task<IReadOnlyList<TicketModel>> GetByClientAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        // VTickets — это представление v_tickets, описанное в скаффолде как
        // HasNoKey().ToView(...). Keyless-сущность нельзя отслеживать и через
        // неё нельзя писать — для чтения вычисленных данных это ровно то,
        // что нужно.
        //
        // Статус приходит ГОТОВЫМ из CASE внутри представления. Мы не считаем
        // «просрочен ли абонемент» в C#: одно определение на всю систему,
        // и по нему же можно фильтровать средствами SQL.
        return await _db.VTickets
            .AsNoTracking()
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.DateStart)
            // Оператор ! — null-forgiving: Postgres не сообщает NOT NULL для
            // колонок представления, поэтому скаффолдер пометил всё как
            // nullable. Мы знаем, что эти значения есть.
            .Select(t => new TicketModel(
                t.Id!.Value,
                t.ClientId!.Value,
                t.TicketTypeName!,
                t.DateStart!.Value,
                t.DateEnd!.Value,
                t.VisitsLimit,
                t.VisitsUsed!.Value,
                t.VisitsRemaining,
                t.Status!))
            .ToListAsync(cancellationToken);
    }
}
