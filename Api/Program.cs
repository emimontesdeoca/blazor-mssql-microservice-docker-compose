using Microsoft.AspNetCore.Http.HttpResults;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/test", () =>
{
    return Results.Ok($"{Guid.NewGuid()}");
});

app.MapPrometheusScrapingEndpoint();

app.MapHealthChecks("/health");

app.Run();