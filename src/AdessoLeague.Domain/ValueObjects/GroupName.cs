namespace AdessoLeague.Domain.ValueObjects;

public sealed record GroupName
{
    private const string Alphabet = "ABCDEFGH";

    private GroupName(string value) => Value = value;

    public static int MaxGroups => Alphabet.Length;

    public string Value { get; }

    public static IReadOnlyList<GroupName> Sequence(int count)
    {
        // Callers reach this through a validated GroupCount, so an out-of-range count is a
        // programming mistake rather than user input.
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, MaxGroups);

        var names = new GroupName[count];
        for (var i = 0; i < count; i++)
        {
            names[i] = new GroupName(Alphabet[i].ToString(CultureInfo.InvariantCulture));
        }

        return names;
    }

    // Rehydration from storage: a value outside the alphabet means the row is corrupt.
    internal static GroupName From(string value)
    {
        if (value is not { Length: 1 } || !Alphabet.Contains(value[0], StringComparison.Ordinal))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Group name must be a single letter from \"{Alphabet}\".");
        }

        return new GroupName(value);
    }

    public override string ToString() => Value;
}
