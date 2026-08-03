using AdessoLeague.Application.Contracts;
using MediatR;

namespace AdessoLeague.Application.Features.Draws.GetDraws;

public sealed record GetDrawsQuery(int Page, int? PageSize) : IRequest<Result<PagedList<DrawSummaryResponse>>>;
