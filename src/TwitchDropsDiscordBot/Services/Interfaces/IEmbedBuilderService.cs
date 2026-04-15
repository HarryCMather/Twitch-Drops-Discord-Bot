using System.Runtime;
using Discord;
using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Services.Interfaces;

public interface IEmbedBuilderService
{
    Embed BuildEmbedForStartupComplete(bool isServerGc, GCLargeObjectHeapCompactionMode lohCompactionMode, bool isDevelopment, int processId, string hostname);

    Embed BuildEmbedForTwitchDropReward(Drop drop);
}
