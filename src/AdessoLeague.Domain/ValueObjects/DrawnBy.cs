namespace AdessoLeague.Domain.ValueObjects;

public sealed record DrawnBy
{
    public const int MaxNameLength = 100;

    private DrawnBy(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; }

    public string LastName { get; }

    public string FullName => $"{FirstName} {LastName}";

    public static Result<DrawnBy> Create(string? firstName, string? lastName)
    {
        var first = Normalize(firstName);
        var last = Normalize(lastName);

        var error = Validate(first, nameof(FirstName)) ?? Validate(last, nameof(LastName));

        return error is null
            ? Result.Success(new DrawnBy(first, last))
            : Result.Failure<DrawnBy>(error);
    }

    private static Error? Validate(string value, string field) => value.Length switch
    {
        0 => Error.Validation($"DrawnBy.{field}Empty", $"{field} is required."),
        > MaxNameLength => Error.Validation(
            $"DrawnBy.{field}TooLong",
            $"{field} cannot be longer than {MaxNameLength} characters."),
        _ => null,
    };

    // Trims the ends and collapses runs of inner whitespace to a single space.
    private static string Normalize(string? value) => value is null
        ? string.Empty
        : string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
