using FluentValidation;
using GymManager.Application.Tickets;

namespace GymManager.Api.Validators;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty().WithMessage("Клиент обязателен.");
        RuleFor(x => x.TicketTypeId).NotEmpty().WithMessage("Тип абонемента обязателен.");

        RuleFor(x => x.DateStart)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1))
            .WithMessage("Дата начала слишком далеко в прошлом.");
    }
}

public sealed class ExtendTicketCommandValidator : AbstractValidator<ExtendTicketCommand>
{
    public ExtendTicketCommandValidator()
    {
        RuleFor(x => x.Days)
            .InclusiveBetween(1, 365)
            .WithMessage("Продлить можно на срок от 1 до 365 дней.");
    }
}