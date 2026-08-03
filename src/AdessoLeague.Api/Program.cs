using System.Text.Json;
using System.Text.Json.Serialization;
using AdessoLeague.Api.Handlers;
using AdessoLeague.Api.HealthChecks;
using AdessoLeague.Api.Middleware;
using AdessoLeague.Api.RateLimiting;
using AdessoLeague.Api.Swagger;
using AdessoLeague.Application;
using AdessoLeague.Application.Options;
using AdessoLeague.Infrastructure;
using AdessoLeague.Infrastructure.Persistence;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using InfrastructureServices = AdessoLeague.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = false);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddOptions<DrawOptions>()
    .Bind(builder.Configuration.GetSection(DrawOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<DrawOptions>, DrawOptionsValidator>();

var drawOptions = builder.Configuration.GetSection(DrawOptions.SectionName).Get<DrawOptions>() ?? new DrawOptions();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(RateLimitPolicies.CreateDraw, limiter =>
    {
        limiter.PermitLimit = drawOptions.RequestsPerMinute;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Adesso World League - Draw API",
        Version = "v1",
        Description = "Distributes 32 teams from 8 countries into 4 or 8 groups by draw.",
    });

    options.SchemaFilter<DrawExampleSchemaFilter>();
    options.SupportNonNullableReferenceTypes();

    var documentation = Path.Combine(AppContext.BaseDirectory, "AdessoLeague.Api.xml");
    if (File.Exists(documentation))
    {
        options.IncludeXmlComments(documentation);
    }
});

builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString(InfrastructureServices.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string \"{InfrastructureServices.ConnectionStringName}\" is missing."),
        name: "postgres",
        tags: ["ready"]);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });

    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<LeagueDbContext>().Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseRateLimiter();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
});

app.MapControllers();

await app.RunAsync();
