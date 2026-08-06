using GymManager.Application.Abstractions;
using GymManager.Application.Tickets;
using GymManager.Application.Visits;
using GymManager.Infrastructure.Entities;
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

    public async Task<TicketState?> GetForUpdateAsync(
    Guid ticketId,
    CancellationToken cancellationToken)
    {
        // FOR UPDATE блокирует строку до конца транзакции: второй запрос по
        // этому же абонементу будет ждать здесь, а не читать устаревшие данные.
        // Именно это исключает гонку «прочитал — проверил — записал».
        //
        // Параметр передан через {0}, а не конкатенацией: FromSqlRaw превращает
        // его в параметр запроса, что исключает SQL-инъекцию.
        var entity = await _db.Tickets
            .FromSqlRaw("SELECT * FROM tickets WHERE id = {0} FOR UPDATE", ticketId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            return null;

        return new TicketState(
            entity.Id,
            entity.ClientId,
            entity.DateStart,
            entity.DateEnd,
            entity.VisitsLimit,
            entity.VisitsUsed,
            entity.IsCancelled);
    }

    public async Task<VisitModel> AddVisitAsync(
        Guid ticketId,
        Guid? trainerId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Сущность уже отслеживается после FromSqlRaw выше, поэтому
        // изменение счётчика попадёт в UPDATE автоматически.
        var ticket = await _db.Tickets.FirstAsync(t => t.Id == ticketId, cancellationToken);

        var visit = new Visit
        {
            TicketId = ticketId,
            TrainerId = trainerId,
            UserId = userId
        };

        _db.Visits.Add(visit);
        ticket.VisitsUsed += 1;

        // Один SaveChanges на обе операции: INSERT в visits и UPDATE счётчика
        // уйдут вместе, внутри транзакции, открытой в UnitOfWork.
        await _db.SaveChangesAsync(cancellationToken);

        return new VisitModel(
            visit.Id,
            visit.TicketId,
            visit.VisitedAt,
            visit.TrainerId,
            visit.UserId,
            ticket.VisitsLimit is null ? null : ticket.VisitsLimit - ticket.VisitsUsed);
    }
}
