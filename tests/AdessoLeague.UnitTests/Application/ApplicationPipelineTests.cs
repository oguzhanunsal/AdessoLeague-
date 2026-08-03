using AdessoLeague.Application;
using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Application.Features.Draws.CreateDraw;
using AdessoLeague.Application.Features.Draws.GetDrawById;
using AdessoLeague.Domain.Draws.Engine;
using AdessoLeague.UnitTests.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdessoLeague.UnitTests.Application;

public sealed class ApplicationPipelineTests
{
    [Fact]
    public async Task Send_WithAValidCommand_ReachesTheHandler()
    {
        var sender = Sender();

        var result = await sender.Send(new CreateDrawCommand(8, "Oğuzhan", "Ünsal"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Groups.Should().HaveCount(8);
    }

    [Fact]
    public async Task Send_WithAnInvalidCommand_IsRejectedByTheValidationBehavior()
    {
        var draws = new FakeDrawRepository();
        var sender = Sender(draws);

        var result = await sender.Send(new CreateDrawCommand(5, string.Empty, "Ünsal"));

        result.Error.Should().BeOfType<ValidationError>();
        draws.Added.Should().BeEmpty();
    }

    [Fact]
    public async Task Send_WithAQuery_ResolvesItsHandlerToo()
    {
        var result = await Sender().Send(new GetDrawByIdQuery(Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Draw.NotFound");
    }

    private static ISender Sender(FakeDrawRepository? draws = null)
    {
        var services = new ServiceCollection();

        services.AddApplication();

        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IDrawRepository>(draws ?? new FakeDrawRepository());
        services.AddSingleton<ITeamRepository>(new FakeTeamRepository(LeagueTestData.Teams));
        services.AddSingleton<IUnitOfWork, FakeUnitOfWork>();
        services.AddSingleton<IDrawRule, OneTeamPerCountryRule>();
        services.AddSingleton<IRandomProvider>(_ => new Draws.SeededTestRandomProvider(20260803));
        services.AddSingleton<IDrawStrategy>(provider => new BacktrackingRoundRobinStrategy(
            provider.GetRequiredService<IRandomProvider>(),
            provider.GetServices<IDrawRule>().ToList()));
        services.AddSingleton<DrawEngine>();

        return services.BuildServiceProvider().GetRequiredService<ISender>();
    }
}
