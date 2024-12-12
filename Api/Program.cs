using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

ConfigureOpenTelemetry(builder);

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

static IHostApplicationBuilder ConfigureOpenTelemetry(IHostApplicationBuilder builder)
{
    var projectName = "Api";
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
    });

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(c => c.AddService(projectName))
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation();
        })
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();
        });

    var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

    if (useOtlpExporter)
    {
        builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }

    return builder;
}

