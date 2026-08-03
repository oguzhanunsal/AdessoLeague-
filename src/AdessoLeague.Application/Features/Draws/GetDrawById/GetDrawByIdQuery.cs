using AdessoLeague.Application.Contracts;
using MediatR;

namespace AdessoLeague.Application.Features.Draws.GetDrawById;

public sealed record GetDrawByIdQuery(Guid Id) : IRequest<Result<DrawResponse>>;
