using AdessoLeague.Domain.Draws.Engine;

namespace AdessoLeague.Infrastructure.Randomization;

public sealed class SeededRandomProvider : IRandomProvider
{
    private readonly Random _random;

    public SeededRandomProvider(int seed)
    {
        Seed = seed;
        _random = new Random(seed);
    }

    public SeededRandomProvider()
        : this(Random.Shared.Next())
    {
    }

    public int Seed { get; }

    public void Shuffle<T>(IList<T> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
