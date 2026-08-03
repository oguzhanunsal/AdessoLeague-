using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Domain.Draws.Engine;

public sealed class OneTeamPerCountryRule : IDrawRule
{
    public bool IsSatisfiedBy(DrawGroup group, Team team) =>
        !group.Teams.Any(placement => placement.Team.CountryId == team.CountryId);
}
