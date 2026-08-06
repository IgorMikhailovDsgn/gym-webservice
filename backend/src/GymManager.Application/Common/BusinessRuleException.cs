namespace GymManager.Application.Common;

public sealed class BusinessRuleException : Exception
{
    /// Машиночитаемый код — фронт может показать разное поведение
    /// для «просрочен» и «исчерпан», не разбирая текст сообщения.
    public string Code { get; }

    public BusinessRuleException(string code, string message) : base(message)
        => Code = code;
}