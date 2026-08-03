using System.Text.Json.Serialization;
using AdessoLeague.Api.Swagger;
using AdessoLeague.Application;
using AdessoLeague.Infrastructure;
using AdessoLeague.Infrastructure.Persistence;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = false);
builder.Services.AddProblemDetails();

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

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

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

app.MapControllers();

await app.RunAsync();
