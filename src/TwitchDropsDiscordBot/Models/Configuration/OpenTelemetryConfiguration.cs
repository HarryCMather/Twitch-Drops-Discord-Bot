namespace TwitchDropsDiscordBot.Models.Configuration;

public sealed record OpenTelemetryConfiguration
{
    public const string SectionKey = "OpenTelemetry";

    /// <summary>
    /// Whether the application should log any recorded Logs, Metrics or Traces to the relevant exporter.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// The OpenTelemetry exporter endpoint url to publish Logs, Metrics or Traces to.
    /// </summary>
    public string ExporterEndpoint { get; init; }
}
