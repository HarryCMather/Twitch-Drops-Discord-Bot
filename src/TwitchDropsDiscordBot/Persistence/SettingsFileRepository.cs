using System.Text.Json;
using TwitchDropsDiscordBot.Models;

namespace TwitchDropsDiscordBot.Persistence;

/// <summary>
/// Implementation of Settings File Repository.
/// Performs file-related actions for retrieving Settings.
/// </summary>
public sealed class SettingsFileRepository
{
    /// <summary>
    /// Reads and Deserializes Settings from Settings.json.
    /// </summary>
    /// <returns></returns>
    public async Task<Settings> GetSettingsFromFileAsync()
    {
        const string filePath = "Settings.json";

        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Settings>(json);
    }
}
