using GymManager.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientRepository _clients;

    public ClientsController(IClientRepository clients) => _clients = clients;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var clients = await _clients.GetAllAsync(cancellationToken);
        return Ok(clients);
    }
}