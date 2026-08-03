using AdessoLeague.Api.Contracts.Requests;
using AdessoLeague.Api.Extensions;
using AdessoLeague.Api.RateLimiting;
using AdessoLeague.Application.Contracts;
using AdessoLeague.Application.Features.Draws.CreateDraw;
using AdessoLeague.Application.Features.Draws.GetDrawById;
using AdessoLeague.Application.Features.Draws.GetDraws;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AdessoLeague.Api.Controllers;

[ApiController]
[ApiVersion(VersionNumber)]
[Route("api/v{version:apiVersion}/draws")]
// No [Produces]: it forces every response onto its first media type, which would rewrite
// ProblemDetails as application/json and stop RFC 9457 clients recognising the error body.
// Media types are documented per status code through [ProducesResponseType] instead.
public sealed class DrawsController(ISender sender) : ControllerBase
{
    private const string VersionNumber = "1.0";

    /// <summary>Runs a new draw and stores it.</summary>
    /// <response code="201">The draw was performed and stored.</response>
    /// <response code="400">The request body failed validation.</response>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.CreateDraw)]
    [ProducesResponseType(typeof(DrawResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DrawResponse>> Create(
        CreateDrawRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new CreateDrawCommand(
            request.GroupCount,
            request.DrawnBy.FirstName,
            request.DrawnBy.LastName);

        var result = await sender.Send(command, cancellationToken);

        // Echo the version exactly as the caller wrote it so Location matches the requested URL.
        var version = RouteData.Values.TryGetValue("version", out var requested) ? requested : VersionNumber;

        return result.ToActionResult(this, draw =>
            CreatedAtAction(nameof(GetById), new { version, id = draw.Id }, draw));
    }

    /// <summary>Returns a stored draw.</summary>
    /// <response code="200">The draw was found.</response>
    /// <response code="404">No draw exists with that id, or the id is not a GUID.</response>
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(DrawResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DrawResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDrawByIdQuery(id), cancellationToken);

        return result.ToActionResult(this);
    }

    /// <summary>Lists past draws, newest first.</summary>
    /// <response code="200">The requested page, together with the total number of draws.</response>
    /// <response code="400">The paging parameters are out of range.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<DrawSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedList<DrawSummaryResponse>>> GetAll(
        int page = 1,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetDrawsQuery(page, pageSize), cancellationToken);

        return result.ToActionResult(this);
    }
}
