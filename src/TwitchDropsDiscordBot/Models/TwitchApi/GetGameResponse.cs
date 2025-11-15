using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TwitchDropsDiscordBot.Models.TwitchApi;

/// <summary>
/// Represents the individual object returned within the "data" array from the GetGames endpoint.
/// As "data" is just a wrapper, JsonDocument will be used to avoid unnecessary processing of this.
/// More information is available in Twitch's Docs:
/// https://dev.twitch.tv/docs/api/reference#get-games
/// </summary>
[SuppressMessage("ReSharper", "InvalidXmlDocComment")]
public sealed class GetGameResponse
{
    /// <summary>
    /// The GameId for the requested game.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public uint Id { get; set; }

    /// <summary>
    /// The Name of the requested game.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// The Box Art Image URL for the requested game.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("box_art_url")]
    // public string BoxArtUrl { get; set; }

    /// <summary>
    /// The IGDB ID for the requested game.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("igdb_id")]
    // public string IgbdId { get; set; }
}
