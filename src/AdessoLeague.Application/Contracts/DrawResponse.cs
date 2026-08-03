namespace AdessoLeague.Application.Contracts;

public sealed record TeamResponse(string Name);

public sealed record GroupResponse(string GroupName, IReadOnlyList<TeamResponse> Teams);

public sealed record DrawnByResponse(string FirstName, string LastName);

public sealed record DrawResponse(
    Guid Id,
    IReadOnlyList<GroupResponse> Groups,
    DrawnByResponse DrawnBy,
    int GroupCount,
    DateTime CreatedAtUtc);

public sealed record DrawSummaryResponse(
    Guid Id,
    DrawnByResponse DrawnBy,
    int GroupCount,
    DateTime CreatedAtUtc);
