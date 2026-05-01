using Serilog;

namespace Tesseract.Web.Common.Observability;

public static class Logging
{
    private static void DefaultLoggingOptions(HostBuilderContext context, IServiceProvider services,
        LoggerConfiguration logger)
    {
        logger.Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.OpenTelemetry();
    }

    extension(IHostBuilder builder)
    {
        public void AddLogging() => builder.UseSerilog(DefaultLoggingOptions);
    }
}