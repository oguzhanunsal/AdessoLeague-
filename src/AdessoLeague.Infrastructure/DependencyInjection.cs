using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Domain.Draws.Engine;
using AdessoLeague.Infrastructure.Persistence;
using AdessoLeague.Infrastructure.Randomization;
using AdessoLeague.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdessoLeague.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Postgres";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string \"{ConnectionStringName}\" is missing.");

        services.AddDbContext<LeagueDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IDrawRepository, DrawRepository>();
        services.AddScoped<IDrawQueries, DrawQueries>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Never a singleton: one provider carries one seed, and one seed identifies one draw.
        services.AddScoped<IRandomProvider>(_ => new SeededRandomProvider());

        services.AddScoped<IDrawRule, OneTeamPerCountryRule>();
        services.AddScoped<IDrawStrategy>(provider => new BacktrackingRoundRobinStrategy(
            provider.GetRequiredService<IRandomProvider>(),
            provider.GetServices<IDrawRule>().ToList()));
        services.AddScoped<DrawEngine>();

        return services;
    }
}
