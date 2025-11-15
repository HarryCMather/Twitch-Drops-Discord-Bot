using System.Text.Json;
using TwitchDropsDiscordBot.Models;

namespace TwitchDropsDiscordBot.Persistence;

public sealed class SettingsFileRepository
{
    public async Task<Settings> GetSettingsFromFileAsync()
    {
        const string filePath = "Settings.json";

        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Settings>(json);
    }
}
