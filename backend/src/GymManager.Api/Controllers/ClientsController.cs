using GymManager.Application.Clients;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientService _clients;

    public ClientsController(IClientService clients) => _clients = clients;

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] ClientQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _clients.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }
}