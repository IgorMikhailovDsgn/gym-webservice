using GymManager.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Middleware;

/// <summary>
/// Единая точка превращения исключений в HTTP-ответы.
///
/// Обработка запроса в ASP.NET Core устроена как конвейер: каждый компонент
/// получает запрос, что-то делает, передаёт дальше и получает управление
/// обратно на пути ответа. Всё, что происходит глубже — контроллер, сервис,
/// репозиторий — оказывается внутри нашего try, поэтому один catch здесь
/// заменяет проверки во всех контроллерах.
///
/// Формат ответа — ProblemDetails (RFC 7807), стандартный для .NET.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Передаём управление следующему в конвейере.
            await _next(context);
        }
        // ПОРЯДОК catch ВАЖЕН: C# берёт первый подходящий блок сверху вниз,
        // поэтому catch (Exception) обязан быть последним.
        //
        // Полное имя FluentValidation.ValidationException — потому что класс
        // с тем же именем есть в System.ComponentModel.DataAnnotations.
        catch (FluentValidation.ValidationException ex)
        {
            // На одно поле может прийти несколько ошибок (пустое И длинное),
            // поэтому группируем: имя свойства -> массив сообщений.
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Ошибка валидации",
                Instance = context.Request.Path
            });
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound,
                "Не найдено", ex.Message);
        }
        catch (Exception ex)
        {
            // Подробности — в лог, наружу обезличенный текст: сообщение
            // исключения может раскрыть имена таблиц, SQL или данные.
            _logger.LogError(ex, "Необработанное исключение при обработке {Path}", context.Request.Path);

            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "Внутренняя ошибка", "Произошла непредвиденная ошибка.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context, int statusCode, string title, string detail)
    {
        // Если ответ уже начал отправляться, заголовки менять поздно.
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        });
    }
}
