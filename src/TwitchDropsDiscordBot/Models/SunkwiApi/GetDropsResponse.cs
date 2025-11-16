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
    public DateTime StartAt { get; set; }

    [JsonPropertyName("endAt")]
    public DateTime EndAt { get; set; }

    [JsonPropertyName("gameDisplayName")]
    public string GameDisplayName { get; set; }

    [JsonPropertyName("rewards")]
    public List<GetDropsReward> Rewards { get; } = [];

    /// <summary>
    /// The Box Art Image URL for the requested game.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("gameBoxArtURL")]
    // public string GameBoxArtUrl { get; set; }

    /// <summary>
    /// The GameId for the requested game.
    /// This is commented out, as it's not currently relevant for my use-case, but may be useful in-future.
    /// </summary>
    // [JsonPropertyName("gameId")]
    // public string GameId { get; set; }
}

public sealed class GetDropsOwner
{
    /// <summary>
    /// The Name of the Owner.
    /// For example: Ubisoft
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// The type of the Owner.
    /// For example: Organization
    /// </summary>
    [JsonPropertyName("__typename")]
    public string Typename { get; set; }

    // [JsonPropertyName("id")]
    // public string Id { get; set; }
}

public sealed class GetDropsReward
{
    /// <summary>
    /// The URL for users to link their accounts to receive drops.
    /// For example: https://drops-register.ubi.com/#/en-US
    /// </summary>
    [JsonPropertyName("accountLinkURL")]
    public string AccountLinkUrl { get; set; }

    /// <summary>
    /// A short description of what the drop is for.
    /// For example: Rainbow Six Siege - MUNICH MAJOR - SEMI FINALS
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// A link containing additional information about the drop, as provided by the publisher.
    /// For exmaple: https://www.ubisoft.com/en-us/help/connectivity-and-performance/article/information-about-twitch-drops-for-ubisoft-games/000065532#:~:text=You%20can%20earn%20in%2Dgame,stream%20from%20a%20participating%20channel.
    /// </summary>
    [JsonPropertyName("detailsURL")]
    public string DetailsUrl { get; set; }

    /// <summary>
    /// DateTime when the reward starts.
    /// </summary>
    [JsonPropertyName("startAt")]
    public DateTime StartAt { get; set; }

    /// <summary>
    /// DateTime when the reward ends.
    /// </summary>
    [JsonPropertyName("endAt")]
    public DateTime EndAt { get; set; }

    /// <summary>
    /// Name of the current drop campaign.
    /// For example: R6S MUNICH 2025 - DAY 7
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Owner (usually an organisation) who has created the current drop.
    /// For example: Owner with the Name "Ubisoft"
    /// </summary>
    [JsonPropertyName("owner")]
    public GetDropsOwner Owner { get; set; }

    /// <summary>
    /// The current state of the drop.
    /// For example: ACTIVE
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    /// All available Time-Based Drops for the current drop reward.
    /// </summary>
    [JsonPropertyName("timeBasedDrops")]
    public List<GetDropsTimeBasedDrop> TimeBasedDrops { get; } = [];

    /// <summary>
    /// The type of the current reward.
    /// For example: DropCampaign
    /// </summary>
    [JsonPropertyName("__typename")]
    public string Typename { get; set; }

    // [JsonPropertyName("id")]
    // public string Id { get; set; }

    // [JsonPropertyName("self")]
    // public GetDropsSelf Self { get; set; }

    // [JsonPropertyName("allow")]
    // public GetDropsAllow Allow { get; set; }

    // [JsonPropertyName("eventBasedDrops")]
    // public List<object> EventBasedDrops { get; } = [];

    // [JsonPropertyName("game")]
    // public GetDropsGame Game { get; set; }

    // [JsonPropertyName("imageURL")]
    // public string ImageURL { get; set; }
}

public sealed class GetDropsTimeBasedDrop
{
    /// <summary>
    /// When the Time-Based Drop starts.
    /// </summary>
    [JsonPropertyName("startAt")]
    public DateTime StartAt { get; set; }

    /// <summary>
    /// When the Time-Based Drop ends.
    /// </summary>
    [JsonPropertyName("endAt")]
    public DateTime EndAt { get; set; }

    /// <summary>
    /// The Name of the Time-Based Drop.
    /// For example: Oryx Uniform Munich 2025
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// The number of minutes of watch time required to unlock the Time-Based Drop.
    /// </summary>
    [JsonPropertyName("requiredMinutesWatched")]
    public ushort RequiredMinutesWatched { get; set; }

    /// <summary>
    /// The type of the current time-based drop.
    /// For example: TimeBasedDrop
    /// </summary>
    [JsonPropertyName("__typename")]
    public string Typename { get; set; }

    // [JsonPropertyName("id")]
    // public string Id { get; set; }

    // [JsonPropertyName("requiredSubs")]
    // public int RequiredSubs { get; set; }

    // [JsonPropertyName("benefitEdges")]
    // public List<GetDropsBenefitEdge> BenefitEdges { get; } = [];

    // [JsonPropertyName("preconditionDrops")]
    // public object PreconditionDrops { get; set; }
}

// public sealed class GetDropsAllow
// {
//     [JsonPropertyName("channels")]
//     public List<GetDropsChannel> Channels { get; } = [];
//
//     [JsonPropertyName("isEnabled")]
//     public bool IsEnabled { get; set; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; set; }
// }

// public sealed class GetDropsSelf
// {
//     [JsonPropertyName("isAccountConnected")]
//     public bool IsAccountConnected { get; set; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; set; }
// }

// public sealed class GetDropsBenefitEdge
// {
//     [JsonPropertyName("benefit")]
//     public GetDropsBenefit Benefit { get; set; }
//
//     [JsonPropertyName("entitlementLimit")]
//     public int EntitlementLimit { get; set; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; set; }
// }

// public sealed class GetDropsBenefit
// {
//     [JsonPropertyName("id")]
//     public string Id { get; set; }
//
//     [JsonPropertyName("createdAt")]
//     public DateTime CreatedAt { get; set; }
//
//     [JsonPropertyName("entitlementLimit")]
//     public int EntitlementLimit { get; set; }
//
//     [JsonPropertyName("game")]
//     public GetDropsGame Game { get; set; }
//
//     [JsonPropertyName("imageAssetURL")]
//     public string ImageAssetUrl { get; set; }
//
//     [JsonPropertyName("isIosAvailable")]
//     public bool IsIosAvailable { get; set; }
//
//     [JsonPropertyName("name")]
//     public string Name { get; set; }
//
//     [JsonPropertyName("ownerOrganization")]
//     public GetDropsOwnerOrganization OwnerOrganization { get; set; }
//
//     [JsonPropertyName("distributionType")]
//     public string DistributionType { get; set; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; set; }
// }

// public sealed class GetDropsChannel
// {
//     [JsonPropertyName("id")]
//     public string Id { get; set; }
//
//     [JsonPropertyName("displayName")]
//     public string DisplayName { get; set; }
//
//     [JsonPropertyName("name")]
//     public string Name { get; set; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; set; }
// }

// public sealed class GetDropsOwnerOrganization
// {
//     [JsonPropertyName("id")]
//     public string Id { get; set; }
//
//     [JsonPropertyName("name")]
//     public string Name { get; set; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; set; }
// }

// public sealed class GetDropsGame
// {
//     [JsonPropertyName("id")]
//     public string Id { get; set; }
//
//     [JsonPropertyName("slug")]
//     public string Slug { get; set; }
//
//     [JsonPropertyName("displayName")]
//     public string DisplayName { get; set; }
//
//     [JsonPropertyName("__typename")]
//     public string Typename { get; set; }
//
//     [JsonPropertyName("name")]
//     public string Name { get; set; }
// }
