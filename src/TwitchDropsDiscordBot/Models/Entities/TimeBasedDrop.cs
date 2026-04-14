namespace TwitchDropsDiscordBot.Models.Entities;

public class TimeBasedDrop
{
    public Guid Id { get; set; }

    public Guid ParentDropId { get; set; }

    public string Name { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public short RequiredMinutesWatched { get; set; }

    public DateTimeOffset? AlertedOn { get; set; }
}
