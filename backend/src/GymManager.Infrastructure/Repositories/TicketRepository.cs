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

    public Task<TicketTypeModel?> GetTypeAsync(Guid ticketTypeId, CancellationToken cancellationToken)
    => _db.TicketTypes
        .AsNoTracking()
        .Where(t => t.Id == ticketTypeId && t.IsActive)
        .Select(t => new TicketTypeModel(t.Id, t.Code, t.Name, t.DurationDays, t.DefaultVisits))
        .FirstOrDefaultAsync(cancellationToken);

    public Task<TicketState?> GetStateAsync(Guid ticketId, CancellationToken cancellationToken)
        => _db.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => new TicketState(
                t.Id, t.ClientId, t.DateStart, t.DateEnd,
                t.VisitsLimit, t.VisitsUsed, t.IsCancelled))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TicketModel> AddAsync(
    Guid clientId, Guid ticketTypeId, DateOnly dateStart, DateOnly dateEnd,
    int? visitsLimit, CancellationToken cancellationToken)
    {
        var entity = new Ticket
        {
            ClientId = clientId,
            TicketTypeId = ticketTypeId,
            DateStart = dateStart,
            DateEnd = dateEnd,
            VisitsLimit = visitsLimit
        };

        _db.Tickets.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return await ReadAsync(entity.Id, cancellationToken);
    }

    public async Task<TicketModel> ExtendAsync(
        Guid ticketId, int days, CancellationToken cancellationToken)
        {
            var entity = await _db.Tickets.FirstAsync(t => t.Id == ticketId, cancellationToken);

            entity.DateEnd = entity.DateEnd.AddDays(days);

            await _db.SaveChangesAsync(cancellationToken);

            return await ReadAsync(ticketId, cancellationToken);
        }

    /// Перечитывает абонемент из представления, чтобы вернуть его
    /// с ВЫЧИСЛЕННЫМ статусом. Считать статус в C# нельзя — это привело бы
    /// ко второму определению одного правила.
    private async Task<TicketModel> ReadAsync(Guid ticketId, CancellationToken cancellationToken)
        => await _db.VTickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => new TicketModel(
                t.Id!.Value, t.ClientId!.Value, t.TicketTypeName!,
                t.DateStart!.Value, t.DateEnd!.Value,
                t.VisitsLimit, t.VisitsUsed!.Value, t.VisitsRemaining, t.Status!))
            .FirstAsync(cancellationToken);
}
