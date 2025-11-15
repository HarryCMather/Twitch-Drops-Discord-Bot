using System.Text.Json.Serialization;

namespace TwitchDropsDiscordBot.Models.TwitchApi;

/// <summary>
/// Represents a successful response from Twitch's OAuth2 Token Endpoint for Server Applications.
/// More information is available in Twitch's Docs:
/// https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#client-credentials-grant-flow
/// </summary>
public sealed class TokenResponse
{
    /// <summary>
    /// The AccessToken to include in requests that require authorization.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    /// <summary>
    /// The number of seconds from when the token was issued that it's valid for.
    /// When handling this value, I'd generally like to refresh this token at some point just before it actually expires.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public uint ExpiresIn { get; set; }

    /// <summary>
    /// The TokenType, which generally seems to be the same as the authentication scheme.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}
