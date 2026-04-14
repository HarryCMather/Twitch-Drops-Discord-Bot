using System.Runtime;
using Discord;
using TwitchDropsDiscordBot.Models.Entities;

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

        embedBuilder.AddField("ServerGC", isServerGc, false)
                    .AddField("LOHCompactionMode", lohCompactionMode, false)
                    .AddField("IsDevelopment", isDevelopment, false)
                    .AddField("ProcessId", processId, false)
                    .AddField("Hostname", hostname, false);

        return embedBuilder.Build();
    }

    public Embed BuildEmbedForTwitchDropReward(Drop drop)
    {
        EmbedBuilder embedBuilder = new();

        AddDropRewardInitialDetails(embedBuilder, drop);
        AddDropRewardBaseDetails(embedBuilder, drop);
        AddDropRewardTimeBasedDrops(embedBuilder, drop.TimeBasedDrops);
        AddDropRewardLinks(embedBuilder, drop);

        return embedBuilder.Build();
    }

    private void AddDropRewardInitialDetails(EmbedBuilder embedBuilder, Drop drop)
    {
        string dropSingularOrPlural = drop.TimeBasedDrops.Count == 1 ? "Drop" : "Drops";

        embedBuilder.WithTitle($"New Active Twitch {dropSingularOrPlural} for {drop.GameName}")
                    .WithDescription(drop.Description)
                    .WithColor(Color.Purple)
                    .WithTimestamp(_timeProvider.GetUtcNow());
    }

    private static void AddDropRewardBaseDetails(EmbedBuilder embedBuilder, Drop drop)
    {
        embedBuilder.AddField("Owner", drop.Owner, false)
                    .AddField("Starts", FormatDateTimeOffset(drop.StartsAt), false)
                    .AddField("Ends", FormatDateTimeOffset(drop.EndsAt), false);
    }

    private static void AddDropRewardTimeBasedDrops(EmbedBuilder embedBuilder, List<TimeBasedDrop> timeBasedDrops)
    {
        foreach (TimeBasedDrop timeBasedDrop in timeBasedDrops)
        {
            embedBuilder.AddField("Reward", $"{timeBasedDrop.Name} - Requires {FormatTimeDuration(timeBasedDrop.RequiredMinutesWatched)} watched", true);
        }
    }

    private static void AddDropRewardLinks(EmbedBuilder embedBuilder, Drop drop)
    {
        if (!string.IsNullOrEmpty(drop.AccountLinkUrl))
        {
            embedBuilder.AddField("Link Account", drop.AccountLinkUrl, false);
        }

        if (!string.IsNullOrEmpty(drop.DetailsUrl))
        {
            embedBuilder.AddField("More Details", drop.DetailsUrl, false);
        }
    }

    private static string FormatDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        return $"{dateTimeOffset.UtcDateTime:ddd dd-MMM-yy HH:mm:ss} (GMT+0)";
    }

    private static string FormatTimeDuration(short minutes)
    {
        if (minutes < 60)
        {
            return $"{minutes} minutes";
        }

        return $"{ConvertMinutesToHours(minutes)} hours";
    }

    private static double ConvertMinutesToHours(short minutes)
    {
        return Math.Round(((double)minutes) / 60.0, 2, MidpointRounding.AwayFromZero);
    }
}
