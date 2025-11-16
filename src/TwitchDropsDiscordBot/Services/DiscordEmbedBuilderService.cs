using System.Runtime;
using Discord;
using TwitchDropsDiscordBot.Models.SunkwiApi;

namespace TwitchDropsDiscordBot.Services;

public sealed class DiscordEmbedBuilderService
{
    private readonly TimeProvider _timeProvider;

    public DiscordEmbedBuilderService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Embed BuildEmbedForStartupComplete(bool isServerGc, GCLargeObjectHeapCompactionMode lohCompactionMode, bool isDevelopment, int processId, string hostname)
    {
        EmbedBuilder embedBuilder = new();

        embedBuilder.WithTitle("Bot started")
                    .WithDescription("Twitch Drops Discord Bot Has Successfully Started...")
                    .WithColor(Color.DarkBlue)
                    .WithTimestamp(_timeProvider.GetUtcNow());

        embedBuilder.AddField("ServerGC", isServerGc, true)
                    .AddField("LOHCompactionMode", lohCompactionMode, true)
                    .AddField("IsDevelopment", isDevelopment, true)
                    .AddField("ProcessId", processId, true)
                    .AddField("Hostname", hostname, true);

        return embedBuilder.Build();
    }

    public Embed BuildEmbedForTwitchDropReward(GetDropsReward dropReward, string gameDisplayName)
    {
        EmbedBuilder embedBuilder = new();

        AddDropRewardInitialDetails(embedBuilder, dropReward, gameDisplayName);
        AddDropRewardBaseDetails(embedBuilder, dropReward);
        AddDropRewardTimeBasedDrops(embedBuilder, dropReward.TimeBasedDrops);
        AddDropRewardLinks(embedBuilder, dropReward);

        return embedBuilder.Build();
    }

    private void AddDropRewardInitialDetails(EmbedBuilder embedBuilder, GetDropsReward dropReward, string gameDisplayName)
    {
        string dropSingularOrPlural = dropReward.TimeBasedDrops.Count == 1 ? "Drop" : "Drops";

        embedBuilder.WithTitle($"New Active Twitch {dropSingularOrPlural} for {gameDisplayName}")
                    .WithDescription(dropReward.Description)
                    .WithColor(Color.Purple)
                    .WithTimestamp(_timeProvider.GetUtcNow());
    }

    private static void AddDropRewardBaseDetails(EmbedBuilder embedBuilder, GetDropsReward dropReward)
    {
        embedBuilder.AddField("Owner", dropReward.Owner.Name, true)
                    .AddField("Starts", FormatDateTimeOffset(dropReward.StartsAt), true)
                    .AddField("Ends", FormatDateTimeOffset(dropReward.EndsAt), true);
    }

    private static void AddDropRewardTimeBasedDrops(EmbedBuilder embedBuilder, List<GetDropsTimeBasedDrop> timeBasedDrops)
    {
        IEnumerable<GetDropsTimeBasedDrop> orderedDrops = timeBasedDrops.OrderBy(drop => drop.StartsAt)
                                                                        .ThenBy(drop => drop.RequiredMinutesWatched);

        foreach (GetDropsTimeBasedDrop timeBasedDrop in orderedDrops)
        {
            embedBuilder.AddField("Reward", $"{timeBasedDrop.Name} - Requires {FormatTimeDuration(timeBasedDrop.RequiredMinutesWatched)} watched", true);
        }
    }

    private static void AddDropRewardLinks(EmbedBuilder embedBuilder, GetDropsReward dropReward)
    {
        if (!string.IsNullOrEmpty(dropReward.AccountLinkUrl))
        {
            embedBuilder.AddField("Link Account", dropReward.AccountLinkUrl, true);
        }

        if (!string.IsNullOrEmpty(dropReward.DetailsUrl))
        {
            embedBuilder.AddField("More Details", dropReward.DetailsUrl, true);
        }
    }

    private static string FormatDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        return $"{dateTimeOffset.UtcDateTime:ddd dd-MMM-yy} (GMT+0)";
    }

    private static string FormatTimeDuration(ushort minutes)
    {
        if (minutes < 60)
        {
            return $"{minutes} minutes";
        }

        return $"{ConvertMinutesToHours(minutes)} hours";
    }

    private static double ConvertMinutesToHours(ushort minutes)
    {
        return Math.Round(((double)minutes) / 60.0, 2, MidpointRounding.AwayFromZero);
    }
}
