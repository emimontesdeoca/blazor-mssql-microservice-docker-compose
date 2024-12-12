using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

ConfigureOpenTelemetry(builder);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/test", () =>
{
    return Results.Ok($"{Guid.NewGuid()}");
});

app.MapHealthChecks("/health");

app.Run();


static IHostApplicationBuilder ConfigureOpenTelemetry(IHostApplicationBuilder builder)
{
    var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")!;

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(c => c.AddService("MyApp"))
        .WithMetrics(metrics =>
        {
            metrics.AddHttpClientInstrumentation()
                   .AddRuntimeInstrumentation()
                   .AddAspNetCoreInstrumentation()
                   .AddConsoleExporter();

            metrics.AddOtlpExporter(x => x.Endpoint = new(endpoint));
        })
        .WithTracing(tracing =>
        {
            tracing.AddHttpClientInstrumentation()
                   .AddAspNetCoreInstrumentation()
                   .AddConsoleExporter();

            tracing.AddOtlpExporter(x => x.Endpoint = new(endpoint));
        });

    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter(x => x.Endpoint = new(endpoint));
    });

    return builder;
}