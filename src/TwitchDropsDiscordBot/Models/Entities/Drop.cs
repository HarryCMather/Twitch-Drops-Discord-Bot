namespace TwitchDropsDiscordBot.Models.Entities;

public class Drop
{
    public Guid Id { get; init; }

    public string GameName { get; init; }

    public short GameId { get; set; }

    public string Owner { get; init; }

    public short DropOwnerId { get; set; }

    public string Name { get; init; }

    public string Description { get; init; }

    public string AccountLinkUrl { get; init; }

    public string DetailsUrl { get; init; }

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset EndsAt { get; init; }

    public List<TimeBasedDrop> TimeBasedDrops { get; set; }

    public string Status { get; init; }
}
