using FluentValidation;
using GymManager.Application.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers;

/// <summary>Абонементы.</summary>
[ApiController]
[Route("api/tickets")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;
    private readonly IValidator<CreateTicketCommand> _createValidator;
    private readonly IValidator<ExtendTicketCommand> _extendValidator;

    public TicketsController(
        ITicketService tickets,
        IValidator<CreateTicketCommand> createValidator,
        IValidator<ExtendTicketCommand> extendValidator)
    {
        _tickets = tickets;
        _createValidator = createValidator;
        _extendValidator = extendValidator;
    }

    /// <summary>Оформить абонемент.</summary>
    /// <remarks>
    /// Срок и лимит посещений копируются из шаблона ticket_types в момент
    /// оформления. Это снимок, а не ссылка: правка справочника не должна
    /// задним числом менять условия уже проданных абонементов.
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketCommand command,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var ticket = await _tickets.CreateAsync(command, cancellationToken);
        return Ok(ticket);
    }

    /// <summary>Продлить абонемент на указанное число дней.</summary>
    /// <remarks>
    /// Просроченный абонемент продлить можно — клиент вернулся.
    /// Отменённый нельзя: отмена была решением, обходить его продлением
    /// неправильно.
    /// </remarks>
    // Глагол в адресе: строгий REST такое не одобряет, но продление
    // не выражается через CRUD — это не «заменить» и не «частично обновить»,
    // а операция с собственным бизнес-смыслом.
    [HttpPost("{id:guid}/extend")]
    public async Task<IActionResult> Extend(
        Guid id,
        [FromBody] ExtendTicketCommand command,
        CancellationToken cancellationToken)
    {
        await _extendValidator.ValidateAndThrowAsync(command, cancellationToken);

        var ticket = await _tickets.ExtendAsync(id, command, cancellationToken);
        return Ok(ticket);
    }

    /// <summary>Список абонементов с фильтрами.</summary>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] TicketQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _tickets.SearchAsync(query, cancellationToken);
        return Ok(result);
    }
}
