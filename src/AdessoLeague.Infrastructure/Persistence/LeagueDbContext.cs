namespace AdessoLeague.Infrastructure.Persistence;

public sealed class LeagueDbContext(DbContextOptions<LeagueDbContext> options) : DbContext(options)
{
    public DbSet<Country> Countries => Set<Country>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Draw> Draws => Set<Draw>();

    public DbSet<DrawGroup> DrawGroups => Set<DrawGroup>();

    public DbSet<DrawGroupTeam> DrawGroupTeams => Set<DrawGroupTeam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeagueDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
