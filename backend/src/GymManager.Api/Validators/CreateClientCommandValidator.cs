using FluentValidation;
using GymManager.Application.Clients;

namespace GymManager.Api.Validators;

/// <summary>
/// Проверка формата входных данных. Живёт в Api, а не в Application,
/// потому что относится к границе системы: данные приходят извне.
/// Бизнес-правила проверяются в сервисах.
/// </summary>
public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна.")
            .MaximumLength(100).WithMessage("Фамилия не длиннее 100 символов.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно.")
            .MaximumLength(100).WithMessage("Имя не длиннее 100 символов.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(100).WithMessage("Отчество не длиннее 100 символов.");

        // MaximumLength(20) повторяет VARCHAR(20) из схемы: БД гарантирует
        // целостность, приложение даёт понятное сообщение до похода в базу.
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Телефон обязателен.")
            .MaximumLength(20).WithMessage("Телефон не длиннее 20 символов.")
            .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Телефон содержит недопустимые символы.");

        // Email необязателен, но если прислан — обязан быть валидным.
        // Без When пустое значение считалось бы ошибкой формата.
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Некорректный адрес электронной почты.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
