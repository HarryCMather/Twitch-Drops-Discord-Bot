using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TwitchDropsDiscordBot.Extensions;

public static class ActivityExtensions
{
    public static Activity StartTrace(this ActivitySource activitySource, string traceName, [CallerMemberName]string name = nameof(StartTrace))
    {
        // This method's name shouldn't be used, and is only acting as a default fallback to prevent errors.
        // If this occurs, something's gone wrong.

        // In reality, it's not realistic that activitySource would be null.  This is mainly to appease static analysis tooling:
        ArgumentNullException.ThrowIfNull(activitySource);

        // The purpose behind this method is to create a server-based activity that has sufficient context to
        // show up within Elastic/Other APM-based solutions without having to look in weird places, as this is something
        // that I've experienced in the past.  For this application, I'm happy for ActivitySource contexts to show up as
        // "requests" for simplicity, even though this isn't a web application.
        Activity activity = activitySource.StartActivity(name, ActivityKind.Server);
        activity?.SetTag("type", "request");
        activity?.SetTag("transaction.name", traceName);
        Activity.Current = activity;
        return activity;
    }
}
