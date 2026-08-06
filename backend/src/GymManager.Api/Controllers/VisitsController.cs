using GymManager.Api.Extensions;
using GymManager.Application.Visits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers;

/// <summary>Посещения тренировок.</summary>
[ApiController]
[Route("api/visits")]
[Authorize]
public sealed class VisitsController : ControllerBase
{
    private readonly IVisitService _visits;

    public VisitsController(IVisitService visits) => _visits = visits;

    /// <summary>Зафиксировать посещение по абонементу.</summary>
    /// <remarks>
    /// Проверяются четыре бизнес-правила: абонемент не отменён, срок начался
    /// и не истёк, лимит посещений не исчерпан. Отказ возвращается как 409
    /// с машиночитаемым кодом причины в поле code.
    ///
    /// Операция выполняется в транзакции с блокировкой строки абонемента,
    /// чтобы два одновременных запроса не списали больше посещений,
    /// чем осталось.
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterVisitCommand command,
        CancellationToken cancellationToken)
    {
        // User — свойство ControllerBase, заполненное после аутентификации.
        // userId берётся из подписанного токена, а не из тела запроса:
        // иначе любой мог бы записать посещение от чужого имени.
        var visit = await _visits.RegisterAsync(command, User.GetUserId(), cancellationToken);

        return Ok(visit);
    }
}
