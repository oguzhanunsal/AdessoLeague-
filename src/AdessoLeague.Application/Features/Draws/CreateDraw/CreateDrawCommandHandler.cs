using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Application.Contracts;
using AdessoLeague.Application.Mapping;
using AdessoLeague.Domain.Draws.Engine;
using MediatR;

namespace AdessoLeague.Application.Features.Draws.CreateDraw;

public sealed class CreateDrawCommandHandler(
    DrawEngine drawEngine,
    ITeamRepository teams,
    IDrawRepository draws,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CreateDrawCommand, Result<DrawResponse>>
{
    public async Task<Result<DrawResponse>> Handle(CreateDrawCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var groupCount = GroupCount.Create(request.GroupCount);
        if (groupCount.IsFailure)
        {
            return Result.Failure<DrawResponse>(groupCount.Error);
        }

        var drawnBy = DrawnBy.Create(request.FirstName, request.LastName);
        if (drawnBy.IsFailure)
        {
            return Result.Failure<DrawResponse>(drawnBy.Error);
        }

        var pool = await teams.GetPoolAsync(cancellationToken);

        var drawn = drawEngine.Run(new DrawRequest(
            Guid.NewGuid(),
            drawnBy.Value,
            groupCount.Value,
            pool,
            timeProvider.GetUtcNow().UtcDateTime));

        if (drawn.IsFailure)
        {
            return Result.Failure<DrawResponse>(drawn.Error);
        }

        await draws.AddAsync(drawn.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(drawn.Value.ToResponse());
    }
}
