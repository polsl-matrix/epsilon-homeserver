using FluentAssertions;
using System.Text.Json;

namespace Tesseract.Web.Tests.Common.Observability;

public class ObservabilityConfigurationTests
{
    private static readonly DirectoryInfo RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void PrometheusConfiguration_CollectorScrapeTarget_TargetsCollectorPrometheusExporter()
    {
        var prometheus = ReadRepositoryFile("observability/prometheus.yml");

        prometheus.Should().Contain("job_name: otel-collector");
        prometheus.Should().Contain("targets: [otel-collector:8889]");
    }

    [Fact]
    public void DockerCompose_CollectorPorts_ExposePrometheusExporterToHost()
    {
        var compose = ReadRepositoryFile("docker-compose.yml");
        var envExample = ReadRepositoryFile(".env.example");

        compose.Should().Contain("127.0.0.1:${OTEL_PROMETHEUS_PORT:-8889}:8889");
        envExample.Should().Contain("OTEL_PROMETHEUS_PORT=8889");
    }

    [Fact]
    public void TelemetryConfiguration_ResourceConfiguration_UsesOtelServiceNameWithApplicationNameFallback()
    {
        var telemetry = ReadRepositoryFile("src/Tesseract.Web/Common/Observability/Telemetry.cs");

        telemetry.Should().Contain("using OpenTelemetry.Resources;");
        telemetry.Should().Contain("ConfigureResource");
        telemetry.Should().Contain("OTEL_SERVICE_NAME");
        telemetry.Should().Contain("builder.Environment.ApplicationName");
    }

    [Fact]
    public void GrafanaDashboard_HttpPanels_UseCollectorPrometheusNormalizedOpenTelemetryMetrics()
    {
        var expressions = ReadDashboardExpressions();

        expressions.Should().Contain("sum(rate(http_server_request_duration_seconds_count[5m]))");
        expressions.Should().Contain("histogram_quantile(0.95, sum(rate(http_server_request_duration_seconds_bucket[5m])) by (le))");
        expressions.Should().Contain("sum(rate(http_server_request_duration_seconds_count{http_response_status_code=~\"5..\"}[5m]))");
    }

    [Fact]
    public void GrafanaDashboard_ApplicationMetricsPanel_UsesServiceNameMetricSelector()
    {
        var expressions = ReadDashboardExpressions();

        expressions.Should().Contain("count({service_name=\"tesseract-homeserver\"})");
    }

    [Theory]
    [InlineData("http_request_duration")]
    [InlineData("http_server_duration")]
    [InlineData("http_server_requests")]
    [InlineData("process_runtime_dotnet")]
    public void GrafanaDashboard_Expressions_DoNotUseObsoleteMetricNamesOrLabels(string obsoleteText)
    {
        var expressions = ReadDashboardExpressions();

        expressions.Should().NotContain(expression => expression.Contains(obsoleteText, StringComparison.Ordinal));
    }

    private static IReadOnlyCollection<string> ReadDashboardExpressions()
    {
        var dashboard = ReadRepositoryFile("observability/grafana/dashboards/tesseract-overview.json");
        using var document = JsonDocument.Parse(dashboard);

        return document.RootElement
            .GetProperty("panels")
            .EnumerateArray()
            .SelectMany(panel => panel.GetProperty("targets").EnumerateArray())
            .Select(target => target.GetProperty("expr").GetString())
            .OfType<string>()
            .ToArray();
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot.FullName, relativePath));
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Tesseract.slnx")))
            {
                return directory;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root containing Tesseract.slnx.");
    }
}