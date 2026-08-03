namespace AdessoLeague.Domain.Leagues;

public sealed class Team
{
    private Team(Guid id, Guid countryId, string name)
    {
        Id = id;
        CountryId = countryId;
        Name = name;
    }

    public Guid Id { get; private set; }

    public Guid CountryId { get; private set; }

    public string Name { get; private set; }

    internal static Team Create(Guid id, Guid countryId, string name) => new(id, countryId, name);
}
