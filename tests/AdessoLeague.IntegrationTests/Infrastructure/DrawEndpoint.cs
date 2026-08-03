namespace AdessoLeague.IntegrationTests.Infrastructure;

internal sealed record DrawnByPayload(string FirstName, string LastName);

internal sealed record CreateDrawPayload(int GroupCount, DrawnByPayload DrawnBy);

internal static class DrawEndpoint
{
    internal const string Path = "/api/v1/draws";

    internal static string For(Guid id) => FormattableString.Invariant($"{Path}/{id}");

    internal static CreateDrawPayload Payload(
        int groupCount,
        string firstName = "Oğuzhan",
        string lastName = "Ünsal") => new(groupCount, new DrawnByPayload(firstName, lastName));
}
