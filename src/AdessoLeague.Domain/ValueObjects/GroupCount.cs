namespace AdessoLeague.Domain.ValueObjects;

public sealed record GroupCount
{
    public const int TotalTeams = 32;

    private static readonly int[] Supported = [4, 8];

    private GroupCount(int value) => Value = value;

    // AsReadOnly, not the array itself: the caller could otherwise cast it back and rewrite the
    // only place n is constrained.
    public static IReadOnlyCollection<int> SupportedValues { get; } = Array.AsReadOnly(Supported);

    public int Value { get; }

    public int TeamsPerGroup => TotalTeams / Value;

    public static Result<GroupCount> Create(int value) => Array.IndexOf(Supported, value) >= 0
        ? Result.Success(new GroupCount(value))
        : Result.Failure<GroupCount>(Error.Validation(
            "GroupCount.Unsupported",
            $"Group count must be 4 or 8, but was {value}."));

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
