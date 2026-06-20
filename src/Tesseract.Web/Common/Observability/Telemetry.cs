using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Tesseract.Web.Common.Observability;

internal static class Telemetry
{
    private static void DefaultResource(IHostApplicationBuilder builder, ResourceBuilder resource)
    {
        var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");

        resource.AddService(
            serviceName: string.IsNullOrWhiteSpace(serviceName)
                ? builder.Environment.ApplicationName
                : serviceName);
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
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(options => DefaultResource(builder, options))
                .WithMetrics(DefaultRuntimeMetrics)
                .WithTracing(options => DefaultRuntimeTracing(builder, options));
        }
    }
}