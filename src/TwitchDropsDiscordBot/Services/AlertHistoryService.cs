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
    /// Checks if we've previously logged that an alert was sent for the provided RewardId/TimeBasedDropId combination.
    /// </summary>
    /// <param name="rewardId"></param>
    /// <param name="timeBasedDropId"></param>
    /// <returns></returns>
    public async Task<bool> HasDropNotificationBeenSentAsync(Guid rewardId, Guid timeBasedDropId)
    {
        string line = GetFormattedLine(rewardId, timeBasedDropId);

        using (StreamReader streamReader = _alertHistoryFileRepository.OpenReadStream())
        {
            while (await streamReader.ReadLineAsync() is { } currentLine)
            {
                if (string.Equals(line, currentLine, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Records that the RewardId/TimeBasedDropId combination has been sent.
    /// </summary>
    /// <param name="rewardId"></param>
    /// <param name="timeBasedDropId"></param>
    public async Task RecordDropNotificationSentAsync(Guid rewardId, Guid timeBasedDropId)
    {
        string line = GetFormattedLine(rewardId, timeBasedDropId);
        await _alertHistoryFileRepository.AppendLineAsync(line);
    }

    private static string GetFormattedLine(Guid rewardId, Guid timeBasedDropId)
    {
        return $"{rewardId}-{timeBasedDropId}";
    }
}
