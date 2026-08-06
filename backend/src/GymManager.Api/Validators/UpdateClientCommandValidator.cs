using FluentValidation;
using GymManager.Application.Clients;

namespace GymManager.Api.Validators;

public sealed class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    private static readonly string[] AllowedStatuses = ["active", "inactive", "blocked"];

    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна.")
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно.")
            .MaximumLength(100);

        RuleFor(x => x.MiddleName).MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Телефон обязателен.")
            .MaximumLength(20)
            .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Телефон содержит недопустимые символы.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Некорректный адрес электронной почты.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        // Повторяет CHECK (status IN (...)) из схемы: без этого правила
        // неверный статус дошёл бы до БД и вернулся пятисоткой вместо 400.
        RuleFor(x => x.Status)
            .Must(s => AllowedStatuses.Contains(s.Trim().ToLowerInvariant()))
            .WithMessage("Допустимые статусы: active, inactive, blocked.");
    }
}
