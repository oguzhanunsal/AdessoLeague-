namespace AdessoLeague.Domain.Draws.Engine;

public interface IDrawStrategy
{
    Result<Draw> Execute(DrawRequest request);
}
