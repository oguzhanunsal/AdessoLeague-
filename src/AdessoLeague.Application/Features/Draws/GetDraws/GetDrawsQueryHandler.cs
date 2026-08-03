using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Application.Contracts;
using MediatR;

namespace AdessoLeague.Application.Features.Draws.GetDraws;

public sealed class GetDrawsQueryHandler(IDrawQueries draws)
    : IRequestHandler<GetDrawsQuery, Result<PagedList<DrawSummaryResponse>>>
{
    public async Task<Result<PagedList<DrawSummaryResponse>>> Handle(
        GetDrawsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var page = await draws.GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Result.Success(page);
    }
}
