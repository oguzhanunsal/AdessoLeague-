using AdessoLeague.Infrastructure;
using AdessoLeague.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Development convenience only. Other environments migrate as a deliberate deployment step.
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<LeagueDbContext>().Database.MigrateAsync();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
