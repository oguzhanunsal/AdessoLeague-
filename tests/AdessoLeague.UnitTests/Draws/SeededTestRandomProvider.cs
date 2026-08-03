using AdessoLeague.Domain.Draws.Engine;

namespace AdessoLeague.UnitTests.Draws;

// Mirrors the production provider so unit tests stay off the Infrastructure reference.
internal sealed class SeededTestRandomProvider(int seed) : IRandomProvider
{
    private readonly Random _random = new(seed);

    public int Seed { get; } = seed;

    public void Shuffle<T>(IList<T> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
