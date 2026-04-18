using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TwitchDropsDiscordBot.Models.Configuration;
using TwitchDropsDiscordBot.Services;

namespace TwitchDropsDiscordBot.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services, IConfiguration configuration, string hostname)
    {
        OpenTelemetryConfiguration openTelemetryConfiguration = configuration.GetRequiredSection(OpenTelemetryConfiguration.SectionKey)
                                                                             .Get<OpenTelemetryConfiguration>();

        if (!openTelemetryConfiguration.Enabled || string.IsNullOrEmpty(openTelemetryConfiguration.ExporterEndpoint))
        {
            // TODO: TRY TO CHANGE THIS TO A LOGGER, THOUGH THIS MAY NOT BE AVAILABLE, AS THE SERVICE PROVIDER HASN'T BEEN BUILT YET:
            Console.WriteLine("Open Telemetry is disabled...");
            return services;
        }

        // Opting to not add Metrics here, as I'm mainly interested in traces and logs at this stage.
        // This can be added here in-future, if required.
        services.AddOpenTelemetry()
                .ConfigureResource(resourceBuilder => resourceBuilder.AddService("TwitchDropsDiscordBot")
                                                                     .AddAttributes([
                                                                         new KeyValuePair<string, object>("service.environment", hostname),
                                                                         new KeyValuePair<string, object>("host.name", hostname),
                                                                         new KeyValuePair<string, object>("deployment.environment", hostname),
                                                                         new KeyValuePair<string, object>("host.hostname", hostname)
                                                                     ]))
                .WithTracing(traceBuilder =>
                {
                    traceBuilder.AddAspNetCoreInstrumentation()
                                .AddHttpClientInstrumentation()
                                .AddNpgsql()
                                .AddSource(TwitchDropsCheckerBackgroundService.TraceName)
                                .AddOtlpExporter(otlpOptions =>
                                {
                                    otlpOptions.Endpoint = new Uri(openTelemetryConfiguration.ExporterEndpoint);
                                    otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                                });
                });

        return services;
    }
}
