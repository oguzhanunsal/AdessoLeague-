using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Application.Contracts;
using AdessoLeague.Application.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace AdessoLeague.Application.Features.Draws.GetDraws;

public sealed class GetDrawsQueryHandler(IDrawQueries draws, IOptions<DrawOptions> options)
    : IRequestHandler<GetDrawsQuery, Result<PagedList<DrawSummaryResponse>>>
{
    public async Task<Result<PagedList<DrawSummaryResponse>>> Handle(
        GetDrawsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var pageSize = request.PageSize ?? options.Value.DefaultPageSize;

        var page = await draws.GetPageAsync(request.Page, pageSize, cancellationToken);

        return Result.Success(page);
    }
}
