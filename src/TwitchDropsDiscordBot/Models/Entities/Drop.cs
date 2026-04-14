namespace TwitchDropsDiscordBot.Models.Entities;

public class Drop
{
    public Guid Id { get; set; }

    public string GameName { get; set; }

    public short GameId { get; set; }

    public string DropOwner { get; set; }

    public short DropOwnerId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string AccountLinkUrl { get; set; }

    public string DetailsUrl { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public List<TimeBasedDrop> TimeBasedDrops { get; set; }
}
