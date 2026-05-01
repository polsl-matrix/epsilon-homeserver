using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Tesseract.Web.Common.Observability;

internal static class Telemetry
{
    private static void DefaultLoggingOptions(OpenTelemetryLoggerOptions logger)
    {
        logger.IncludeScopes = true;
        logger.IncludeFormattedMessage = true;
        logger.AddOtlpExporter();
    }

    private static void DefaultRuntimeMetrics(MeterProviderBuilder options)
    {
        options.AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter();
    }

    private static void DefaultRuntimeTracing(
        IHostApplicationBuilder builder, TracerProviderBuilder options)
    {
        if (builder.Environment.IsDevelopment())
        {
            options.SetSampler<AlwaysOnSampler>();
        }

        options.AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    }

    extension(IHostApplicationBuilder builder)
    {
        public void AddOpenTelemetry()
        {
            builder.Logging.AddOpenTelemetry(DefaultLoggingOptions);

            builder.Services.AddOpenTelemetry()
                .WithMetrics(DefaultRuntimeMetrics)
                .WithTracing(options => DefaultRuntimeTracing(builder, options));
        }
    }
}