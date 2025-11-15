using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TwitchDropsDiscordBot.Models.TwitchApi;

/// <summary>
/// Represents the objects returned from the GetStreams endpoint.
/// More information is available in Twitch's Docs:
/// https://dev.twitch.tv/docs/api/reference#get-streams
/// </summary>
public sealed class GetStreamsResponse
{
    [JsonPropertyName("data")]
    public List<GetStreamResponse> Data { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationResponse Pagination { get; set; }
}

[SuppressMessage("ReSharper", "InvalidXmlDocComment")]
public sealed class GetStreamResponse
{
    /// <summary>
    /// The list of tags for the requested stream.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    /// <summary>
    /// The ID of the requested stream.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("id")]
    // public uint Id { get; set; }

    /// <summary>
    /// The UserId of the requested streamer.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("user_id")]
    // public string UserId { get; set; }

    /// <summary>
    /// The UserLogin of the requested streamer.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("user_login")]
    // public string UserLogin { get; set; }

    /// <summary>
    /// The UserName of the requested streamer.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("user_name")]
    // public string UserName { get; set; }

    /// <summary>
    /// The Game ID of the requested stream.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("game_id")]
    // public string GameId { get; set; }

    /// <summary>
    /// The Game Name of the requested stream.
    /// This should currently only return a single entry, as I'm only passing in a single GameId.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("game_name")]
    // public string GameName { get; set; }

    /// <summary>
    /// The type of the requested stream.
    /// This should currently only return "live" as this is in the request filters.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("type")]
    // public string Type { get; set; }

    /// <summary>
    /// The Title of the requested stream.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("title")]
    // public string Title { get; set; }

    /// <summary>
    /// The current viewer count for the requested stream.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("viewer_count")]
    // public int ViewerCount { get; set; }

    /// <summary>
    /// When the requested stream started.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("started_at")]
    // public DateTime StartedAt { get; set; }

    /// <summary>
    /// The language of the requested stream.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("language")]
    // public string Language { get; set; }

    /// <summary>
    /// The thumbnail for the current stream.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("thumbnail_url")]
    // public string ThumbnailUrl { get; set; }

    /// <summary>
    /// The list of tag IDs for the requested stream.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("tag_ids")]
    // public List<string> TagIds { get; set; }

    /// <summary>
    /// Whether the stream is marked as mature.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("is_mature")]
    // public bool IsMature { get; set; }
}
