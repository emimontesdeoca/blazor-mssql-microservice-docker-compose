using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(x =>
{
    x.AddOtlpExporter(a =>
    {
        a.Endpoint = new Uri("http://seq:5341/ingest/otlp/v1/logs");
        a.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        a.Headers = "X-Seq-ApiKey=12345678901234567890";
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/test", () =>
{
    return Results.Ok($"{Guid.NewGuid()}");
});


app.MapHealthChecks("/health");

app.Run();