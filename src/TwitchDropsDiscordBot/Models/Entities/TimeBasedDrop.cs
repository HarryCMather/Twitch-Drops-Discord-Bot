namespace TwitchDropsDiscordBot.Models.Entities;

public class TimeBasedDrop
{
    public Guid Id { get; set; }

    public Guid ParentDropId { get; set; }

    public string Name { get; set; }

    public DateTimeOffset StarsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public short RequiredMinutesWatched { get; set; }
}
