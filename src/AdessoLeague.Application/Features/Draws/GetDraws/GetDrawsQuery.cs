using AdessoLeague.Application.Contracts;
using MediatR;

namespace AdessoLeague.Application.Features.Draws.GetDraws;

public sealed record GetDrawsQuery(int Page, int PageSize) : IRequest<Result<PagedList<DrawSummaryResponse>>>
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}
