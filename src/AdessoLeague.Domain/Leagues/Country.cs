namespace AdessoLeague.Domain.Leagues;

public sealed class Country
{
    private readonly List<Team> _teams = [];

    private Country() => Name = null!;

    private Country(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public IReadOnlyCollection<Team> Teams => _teams.AsReadOnly();

    public static Country Create(Guid id, string name, IEnumerable<(Guid Id, string Name)> teams)
    {
        var country = new Country(id, name);

        foreach (var (teamId, teamName) in teams)
        {
            country._teams.Add(Team.Create(teamId, id, teamName));
        }

        return country;
    }
}
