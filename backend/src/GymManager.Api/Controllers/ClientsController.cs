using FluentValidation;
using GymManager.Application.Clients;
using GymManager.Application.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers;

/// <summary>Клиенты фитнес-центра.</summary>
[ApiController]
[Route("api/clients")]
[Authorize]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientService _clients;
    private readonly ITicketService _tickets;
    private readonly IValidator<CreateClientCommand> _createValidator;
    private readonly IValidator<UpdateClientCommand> _updateValidator;

    public ClientsController(
        IClientService clients,
        ITicketService tickets,
        IValidator<CreateClientCommand> createValidator,
        IValidator<UpdateClientCommand> updateValidator)
    {
        _clients = clients;
        _tickets = tickets;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Список клиентов с поиском, фильтром по статусу и пагинацией.</summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] ClientQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _clients.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Карточка клиента.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        // Если клиента нет, сервис бросит NotFoundException, а middleware
        // превратит её в 404. Контроллер об этом не думает.
        var client = await _clients.GetByIdAsync(id, cancellationToken);
        return Ok(client);
    }

    /// <summary>История абонементов клиента.</summary>
    [HttpGet("{id:guid}/tickets")]
    public async Task<IActionResult> GetTickets(Guid id, CancellationToken cancellationToken)
    {
        var tickets = await _tickets.GetByClientAsync(id, cancellationToken);
        return Ok(tickets);
    }

    /// <summary>Создание клиента.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateClientCommand command,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var client = await _clients.CreateAsync(command, cancellationToken);

        // 201 Created + заголовок Location с адресом созданного ресурса.
        // nameof вместо строки: при переименовании метода компилятор поправит.
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    /// <summary>Редактирование клиента.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateClientCommand command,
        CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var client = await _clients.UpdateAsync(id, command, cancellationToken);

        return Ok(client);
    }
}