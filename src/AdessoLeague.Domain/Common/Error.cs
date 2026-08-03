namespace AdessoLeague.Domain.Common;

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
}

public record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
}

// Carries every failure so the API can report them per field instead of only the first one.
public sealed record ValidationError(IReadOnlyList<Error> Errors)
    : Error("Validation.Failed", "One or more validation errors occurred.", ErrorType.Validation);
