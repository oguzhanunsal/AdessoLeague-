using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Application.Contracts;
using MediatR;

namespace AdessoLeague.Application.Features.Draws.GetDrawById;

public sealed class GetDrawByIdQueryHandler(IDrawQueries draws)
    : IRequestHandler<GetDrawByIdQuery, Result<DrawResponse>>
{
    public async Task<Result<DrawResponse>> Handle(GetDrawByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var draw = await draws.GetByIdAsync(request.Id, cancellationToken);

        return draw is null
            ? Result.Failure<DrawResponse>(Error.NotFound("Draw.NotFound", $"No draw with id {request.Id} exists."))
            : Result.Success(draw);
    }
}
