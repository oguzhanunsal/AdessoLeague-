using AdessoLeague.Application.Contracts;
using MediatR;

namespace AdessoLeague.Application.Features.Draws.CreateDraw;

public sealed record CreateDrawCommand(int GroupCount, string FirstName, string LastName)
    : IRequest<Result<DrawResponse>>;
