using FluentValidation;
using GymManager.Application.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers;

/// Абонементы.
[ApiController]
[Route("api/tickets")]
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

    /// Оформить абонемент.
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketCommand command,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var ticket = await _tickets.CreateAsync(command, cancellationToken);
        return Ok(ticket);
    }

    /// Продлить абонемент.
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
}