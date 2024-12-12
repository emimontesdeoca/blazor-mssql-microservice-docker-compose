using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(c => c.AddService("MyApp"))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();

        metrics.AddOtlpExporter();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();

        tracing.AddOtlpExporter();
    });

builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter());

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/test", (ILogger<Program> logger) =>
{
    var guid = Guid.NewGuid();

    logger.LogInformation("Generated GUID: {Guid}", guid);

    return Results.Ok(guid);
});


app.MapHealthChecks("/health");

app.Run();
