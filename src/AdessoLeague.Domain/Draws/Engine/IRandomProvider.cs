namespace AdessoLeague.Domain.Draws.Engine;

public interface IRandomProvider
{
    int Seed { get; }

    void Shuffle<T>(IList<T> items);
}
