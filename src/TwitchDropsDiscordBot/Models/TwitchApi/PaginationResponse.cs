using System.Text.Json.Serialization;

namespace TwitchDropsDiscordBot.Models.TwitchApi;

/// <summary>
/// Represents pagination-specific properties within one of Twitch's API responses.
/// </summary>
public sealed class PaginationResponse
{
    /// <summary>
    /// The cursor to be specified in the next request, usually in "after" (assuming more responses are available).
    /// Will be null or empty if no more results are available.
    /// </summary>
    [JsonPropertyName("cursor")]
    public string Cursor { get; set; }
}
