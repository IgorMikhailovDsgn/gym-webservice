using GymManager.Application.Visits;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers;

/// Посещения тренировок.
[ApiController]
[Route("api/visits")]
public sealed class VisitsController : ControllerBase
{
    private readonly IVisitService _visits;

    public VisitsController(IVisitService visits) => _visits = visits;

    /// Зафиксировать посещение по абонементу.
    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterVisitCommand command,
        CancellationToken cancellationToken)
    {
        var visit = await _visits.RegisterAsync(command, cancellationToken);
        return Ok(visit);
    }
}