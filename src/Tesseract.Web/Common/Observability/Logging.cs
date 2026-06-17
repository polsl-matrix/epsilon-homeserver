using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.OpenTelemetry;

namespace Tesseract.Web.Common.Observability;

public static class Logging
{
    private const string DefaultOtlpEndpoint = "http://localhost:4317";

    private static void DefaultLoggingOptions(
        HostBuilderContext context, IServiceProvider services, LoggerConfiguration logger)
    {
        var otlpEndpoint = context.Configuration["Observability:OtlpEndpoint"] ?? DefaultOtlpEndpoint;

        logger.Enrich.FromLogContext()
            .Enrich.WithSpan()
            .WriteTo.Console()
            .WriteTo.OpenTelemetry(endpoint: otlpEndpoint, protocol: OtlpProtocol.Grpc);
    }

    extension(IHostBuilder builder)
    {
        public void AddLogging() => builder.UseSerilog(DefaultLoggingOptions);
    }
}