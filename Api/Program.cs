using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

ConfigureOpenTelemetry2(builder);

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
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(c => c.AddService("MyApp"))
        .WithMetrics(metrics =>
        {
            metrics.AddHttpClientInstrumentation()
                   .AddRuntimeInstrumentation()
                   .AddAspNetCoreInstrumentation();
        })
        .WithTracing(tracing =>
        {
            tracing.AddHttpClientInstrumentation()
                   .AddAspNetCoreInstrumentation();
        });

    // Use the OTLP exporter if the endpoint is configured.
    var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
    if (useOtlpExporter)
    {
        builder.Services
            .AddOpenTelemetry()
            .UseOtlpExporter();
    }
    else
    {
        Console.WriteLine("OTLP Exporter not configured");
    }

    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter();
    });

    return builder;
}

static IHostApplicationBuilder ConfigureOpenTelemetry2(IHostApplicationBuilder builder)
{
    //var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")!;

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(c => c.AddService("MyApp"))
        .WithMetrics(metrics =>
        {
            metrics.AddHttpClientInstrumentation()
                   .AddRuntimeInstrumentation()
                   .AddAspNetCoreInstrumentation()
                   .AddConsoleExporter();

            metrics.AddOtlpExporter();
        })
        .WithTracing(tracing =>
        {
            tracing.AddHttpClientInstrumentation()
                   .AddAspNetCoreInstrumentation()
                   .AddConsoleExporter();

            tracing.AddOtlpExporter();
        });

    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter();
    });

    return builder;
}