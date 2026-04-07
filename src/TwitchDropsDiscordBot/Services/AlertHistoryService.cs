using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

/// <summary>
/// Implementation of Alert History Service.
/// Used for verifying whether alerts have been sent, or for logging that an alert was sent.
/// </summary>
public sealed class AlertHistoryService
{
    private readonly AlertHistoryFileRepository _alertHistoryFileRepository;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="alertHistoryFileRepository"></param>
    public AlertHistoryService(AlertHistoryFileRepository alertHistoryFileRepository)
    {
        _alertHistoryFileRepository = alertHistoryFileRepository;
    }

    /// <summary>
    /// Records that the RewardId/TimeBasedDropId combination has been sent.
    /// </summary>
    /// <param name="rewardId"></param>
    /// <param name="timeBasedDropId"></param>
    public async Task RecordDropNotificationSentAsync(Guid rewardId, IEnumerable<Guid> timeBasedDropId)
    {
        IEnumerable<string> formattedLines = timeBasedDropId.Select(dropId => GetFormattedLine(rewardId, dropId));
        await _alertHistoryFileRepository.AppendLineAsync(formattedLines);
    }

    private static string GetFormattedLine(Guid rewardId, Guid timeBasedDropId)
    {
        return $"{rewardId}-{timeBasedDropId}";
    }
}
