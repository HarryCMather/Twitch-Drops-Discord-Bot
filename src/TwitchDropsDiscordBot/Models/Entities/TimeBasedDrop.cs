namespace TwitchDropsDiscordBot.Models.Entities;

public class TimeBasedDrop
{
    public Guid Id { get; init; }

    public Guid ParentDropId { get; init; }

    public string Name { get; init; }

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset EndsAt { get; init; }

    public short RequiredMinutesWatched { get; init; }

    public DateTimeOffset? AlertedOn { get; set; }
}
