using System.Globalization;
using AdessoLeague.Application.Options;
using AdessoLeague.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using InfrastructureServices = AdessoLeague.Infrastructure.DependencyInjection;

namespace AdessoLeague.IntegrationTests.Infrastructure;

public sealed class LeagueApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const int RequestsPerMinuteForTests = 10_000;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("adesso_league_tests")
        .WithUsername("adesso")
        .WithPassword("adesso")
        .Build();

    private readonly CancellationTokenSource _cancellation = new();

    private HttpClient? _client;

    /// <summary>Cancelled when the fixture is torn down, so a hung test cannot outlive the container.</summary>
    public CancellationToken Cancellation => _cancellation.Token;

    public HttpClient Client => _client ??= CreateClient();

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LeagueDbContext>();

        // Truncating draws cascades into draw_groups and draw_group_teams, which is exactly the set
        // of rows a test produces. countries and teams are migration seed data and must survive.
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE draws CASCADE;", cancellationToken);
    }

    public async Task<TResult> ExecuteDbAsync<TResult>(
        Func<LeagueDbContext, CancellationToken, Task<TResult>> query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        await using var scope = Services.CreateAsyncScope();

        return await query(scope.ServiceProvider.GetRequiredService<LeagueDbContext>(), cancellationToken);
    }

    public async Task<TResult> ExecuteScopedAsync<TService, TResult>(
        Func<TService, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken)
        where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(action);

        await using var scope = Services.CreateAsyncScope();

        return await action(scope.ServiceProvider.GetRequiredService<TService>(), cancellationToken);
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync(Cancellation);

        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LeagueDbContext>();

        // The API migrates itself only in Development. Doing it here keeps the suite identical on a
        // developer machine and in CI.
        await context.Database.MigrateAsync(Cancellation);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _cancellation.CancelAsync();
        _cancellation.Dispose();

        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Testing");

        // Both values are read while the host is being built - the health check needs the connection
        // string, the rate limiter needs the permit count - so they cannot be replaced afterwards.
        builder.UseSetting(
            $"ConnectionStrings:{InfrastructureServices.ConnectionStringName}",
            _postgres.GetConnectionString());

        // The production limit of 30 draws per minute would turn shared-fixture runs red at random.
        builder.UseSetting(
            $"{DrawOptions.SectionName}:{nameof(DrawOptions.RequestsPerMinute)}",
            RequestsPerMinuteForTests.ToString(CultureInfo.InvariantCulture));
    }
}
