using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Application.Contracts;
using AdessoLeague.Application.Mapping;
using MediatR;

namespace AdessoLeague.Application.Features.Draws.GetDraws;

public sealed class GetDrawsQueryHandler(IDrawRepository draws)
    : IRequestHandler<GetDrawsQuery, Result<PagedList<DrawSummaryResponse>>>
{
    public async Task<Result<PagedList<DrawSummaryResponse>>> Handle(
        GetDrawsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var totalCount = await draws.CountAsync(cancellationToken);

        var page = await draws.GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Result.Success(new PagedList<DrawSummaryResponse>(
            page.Select(draw => draw.ToSummaryResponse()).ToList(),
            request.Page,
            request.PageSize,
            totalCount));
    }
}
