using System.Text.Json.Serialization;
// ReSharper disable InvalidXmlDocComment

namespace TwitchDropsDiscordBot.Models.SunkwiApi;

/// <summary>
/// Represents the Sunkwi GetDrops endpoint response.
/// More information is available in on the Sunkwi GitHub:
/// https://github.com/SunkwiBOT/twitch-drops-api
/// Not all response properties are used, and I've commented these out instead of just setting [JsonIgnore] on the relevant properties,
/// as this avoids the potential for them to be accidentally accessed when ignored.
/// </summary>
public sealed class GetDropsResponse
{
    [JsonPropertyName("startAt")]
    public DateTimeOffset StartsAt { get; init; }

    [JsonPropertyName("endAt")]
    public DateTimeOffset EndsAt { get; init; }

    [JsonPropertyName("gameDisplayName")]
    public string GameDisplayName { get; init; }

    [JsonPropertyName("rewards")]
    public List<GetDropsReward> Rewards { get; init; }

    /// <summary>
    /// The Box Art Image URL for the requested game.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("gameBoxArtURL")]
    // public string GameBoxArtUrl { get; init; }

    /// <summary>
    /// The GameId for the requested game.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("gameId")]
    // public string GameId { get; init; }
}

public sealed class GetDropsOwner
{
    /// <summary>
    /// The Name of the Owner.
    /// For example: Ubisoft
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; }

    /// <summary>
    /// The type of the Owner.
    /// For example: Organization
    /// </summary>
    [JsonPropertyName("__typename")]
    public string Typename { get; init; }

    // [JsonPropertyName("id")]
    // public string Id { get; init; }
}

public sealed class GetDropsReward
{
    /// <summary>
    /// The ID for the current reward.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// The URL for users to link their accounts to receive drops.
    /// For example: https://drops-register.ubi.com/#/en-US
    /// </summary>
    [JsonPropertyName("accountLinkURL")]
    public string AccountLinkUrl { get; init; }

    /// <summary>
    /// A short description of what the drop is for.
    /// For example: Rainbow Six Siege - MUNICH MAJOR - SEMI FINALS
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; }

    /// <summary>
    /// A link containing additional information about the drop, as provided by the publisher.
    /// For exmaple: https://www.ubisoft.com/en-us/help/connectivity-and-performance/article/information-about-twitch-drops-for-ubisoft-games/000065532#:~:text=You%20can%20earn%20in%2Dgame,stream%20from%20a%20participating%20channel.
    /// </summary>
    [JsonPropertyName("detailsURL")]
    public string DetailsUrl { get; init; }

    /// <summary>
    /// DateTime when the reward starts.
    /// </summary>
    [JsonPropertyName("startAt")]
    public DateTimeOffset StartsAt { get; init; }

    /// <summary>
    /// DateTime when the reward ends.
    /// </summary>
    [JsonPropertyName("endAt")]
    public DateTimeOffset EndsAt { get; init; }

    /// <summary>
    /// Name of the current drop campaign.
    /// For example: R6S MUNICH 2025 - DAY 7
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; }

    /// <summary>
    /// Owner (usually an organisation) who has created the current drop.
    /// For example: Owner with the Name "Ubisoft"
    /// </summary>
    [JsonPropertyName("owner")]
    public GetDropsOwner Owner { get; init; }

    /// <summary>
    /// The current state of the drop.
    /// For example: ACTIVE
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; }

    /// <summary>
    /// All available Time-Based Drops for the current drop reward.
    /// </summary>
    [JsonPropertyName("timeBasedDrops")]
    public List<GetDropsTimeBasedDrop> TimeBasedDrops { get; init; }

    /// <summary>
    /// The type of the current reward.
    /// For example: DropCampaign
    /// </summary>
    [JsonPropertyName("__typename")]
    public string Typename { get; init; }

    // [JsonPropertyName("self")]
    // public GetDropsSelf Self { get; init; }

    // [JsonPropertyName("allow")]
    // public GetDropsAllow Allow { get; init; }

    // [JsonPropertyName("eventBasedDrops")]
    // public List<object> EventBasedDrops { get; init; }

    // [JsonPropertyName("game")]
    // public GetDropsGame Game { get; init; }

    // [JsonPropertyName("imageURL")]
    // public string ImageURL { get; init; }
}

public sealed class GetDropsTimeBasedDrop
{
    /// <summary>
    /// The ID for the current Time-Based Drop.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// When the Time-Based Drop starts.
    /// </summary>
    [JsonPropertyName("startAt")]
    public DateTimeOffset StartsAt { get; init; }

    /// <summary>
    /// When the Time-Based Drop ends.
    /// </summary>
    [JsonPropertyName("endAt")]
    public DateTimeOffset EndsAt { get; init; }

    /// <summary>
    /// The Name of the Time-Based Drop.
    /// For example: Oryx Uniform Munich 2025
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; }

    /// <summary>
    /// The number of minutes of watch time required to unlock the Time-Based Drop.
    /// </summary>
    [JsonPropertyName("requiredMinutesWatched")]
    public short RequiredMinutesWatched { get; init; }

    /// <summary>
    /// The type of the current time-based drop.
    /// For example: TimeBasedDrop
    /// </summary>
    [JsonPropertyName("__typename")]
    public string Typename { get; init; }

    // [JsonPropertyName("requiredSubs")]
    // public int RequiredSubs { get; init; }

    // [JsonPropertyName("benefitEdges")]
    // public List<GetDropsBenefitEdge> BenefitEdges { get; init; }

    // [JsonPropertyName("preconditionDrops")]
    // public object PreconditionDrops { get; init; }
}

// public sealed class GetDropsAllow
// {
//     [JsonPropertyName("channels")]
//     public List<GetDropsChannel> Channels { get; init; }
//
//     [JsonPropertyName("isEnabled")]
//     public bool IsEnabled { get; init; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; init; }
// }

// public sealed class GetDropsSelf
// {
//     [JsonPropertyName("isAccountConnected")]
//     public bool IsAccountConnected { get; init; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; init; }
// }

// public sealed class GetDropsBenefitEdge
// {
//     [JsonPropertyName("benefit")]
//     public GetDropsBenefit Benefit { get; init; }
//
//     [JsonPropertyName("entitlementLimit")]
//     public int EntitlementLimit { get; init; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; init; }
// }

// public sealed class GetDropsBenefit
// {
//     [JsonPropertyName("id")]
//     public string Id { get; init; }
//
//     [JsonPropertyName("createdAt")]
//     public DateTimeOffset CreatedAt { get; init; }
//
//     [JsonPropertyName("entitlementLimit")]
//     public int EntitlementLimit { get; init; }
//
//     [JsonPropertyName("game")]
//     public GetDropsGame Game { get; init; }
//
//     [JsonPropertyName("imageAssetURL")]
//     public string ImageAssetUrl { get; init; }
//
//     [JsonPropertyName("isIosAvailable")]
//     public bool IsIosAvailable { get; init; }
//
//     [JsonPropertyName("name")]
//     public string Name { get; init; }
//
//     [JsonPropertyName("ownerOrganization")]
//     public GetDropsOwnerOrganization OwnerOrganization { get; init; }
//
//     [JsonPropertyName("distributionType")]
//     public string DistributionType { get; init; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; init; }
// }

// public sealed class GetDropsChannel
// {
//     [JsonPropertyName("id")]
//     public string Id { get; init; }
//
//     [JsonPropertyName("displayName")]
//     public string DisplayName { get; init; }
//
//     [JsonPropertyName("name")]
//     public string Name { get; init; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; init; }
// }

// public sealed class GetDropsOwnerOrganization
// {
//     [JsonPropertyName("id")]
//     public string Id { get; init; }
//
//     [JsonPropertyName("name")]
//     public string Name { get; init; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; init; }
// }

// public sealed class GetDropsGame
// {
//     [JsonPropertyName("id")]
//     public string Id { get; init; }
//
//     [JsonPropertyName("slug")]
//     public string Slug { get; init; }
//
//     [JsonPropertyName("displayName")]
//     public string DisplayName { get; init; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; init; }
//
//     [JsonPropertyName("name")]
//     public string Name { get; init; }
// }
