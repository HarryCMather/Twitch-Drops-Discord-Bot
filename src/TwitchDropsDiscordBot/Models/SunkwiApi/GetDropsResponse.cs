using System.Text.Json.Serialization;

namespace TwitchDropsDiscordBot.Models.SunkwiApi;

/// <summary>
/// Represents the Sunkwi GetDrops endpoint response.
/// More information is available in on the Sunkwi GitHub:
/// https://github.com/SunkwiBOT/twitch-drops-api
/// </summary>
public sealed class GetDropsResponse
{
    [JsonPropertyName("endAt")]
    public DateTime EndAt { get; set; }

    [JsonPropertyName("gameBoxArtURL")]
    public string GameBoxArtURL { get; set; }

    [JsonPropertyName("gameDisplayName")]
    public string GameDisplayName { get; set; }

    [JsonPropertyName("gameId")]
    public string GameId { get; set; }

    [JsonPropertyName("rewards")]
    public List<Reward> Rewards { get; } = new List<Reward>();

    [JsonPropertyName("startAt")]
    public DateTime StartAt { get; set; }
}

public class Allow
{
    [JsonPropertyName("channels")]
    public List<Channel> Channels { get; } = new List<Channel>();

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}

public class Benefit
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("entitlementLimit")]
    public int EntitlementLimit { get; set; }

    [JsonPropertyName("game")]
    public Game Game { get; set; }

    [JsonPropertyName("imageAssetURL")]
    public string ImageAssetURL { get; set; }

    [JsonPropertyName("isIosAvailable")]
    public bool IsIosAvailable { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("ownerOrganization")]
    public OwnerOrganization OwnerOrganization { get; set; }

    [JsonPropertyName("distributionType")]
    public string DistributionType { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}

public class BenefitEdge
{
    [JsonPropertyName("benefit")]
    public Benefit Benefit { get; set; }

    [JsonPropertyName("entitlementLimit")]
    public int EntitlementLimit { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}

public class Channel
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}

public class Game
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class Owner
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}

public class OwnerOrganization
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}

public class Reward
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("self")]
    public Self Self { get; set; }

    [JsonPropertyName("allow")]
    public Allow Allow { get; set; }

    [JsonPropertyName("accountLinkURL")]
    public string AccountLinkURL { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("detailsURL")]
    public string DetailsURL { get; set; }

    [JsonPropertyName("endAt")]
    public DateTime EndAt { get; set; }

    [JsonPropertyName("eventBasedDrops")]
    public List<object> EventBasedDrops { get; } = new List<object>();

    [JsonPropertyName("game")]
    public Game Game { get; set; }

    [JsonPropertyName("imageURL")]
    public string ImageURL { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("owner")]
    public Owner Owner { get; set; }

    [JsonPropertyName("startAt")]
    public DateTime StartAt { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("timeBasedDrops")]
    public List<TimeBasedDrop> TimeBasedDrops { get; } = new List<TimeBasedDrop>();

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}

public class Self
{
    [JsonPropertyName("isAccountConnected")]
    public bool IsAccountConnected { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}

public class TimeBasedDrop
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("requiredSubs")]
    public int RequiredSubs { get; set; }

    [JsonPropertyName("benefitEdges")]
    public List<BenefitEdge> BenefitEdges { get; } = [];

    [JsonPropertyName("endAt")]
    public DateTime EndAt { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("preconditionDrops")]
    public object PreconditionDrops { get; set; }

    [JsonPropertyName("requiredMinutesWatched")]
    public int RequiredMinutesWatched { get; set; }

    [JsonPropertyName("startAt")]
    public DateTime StartAt { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}
