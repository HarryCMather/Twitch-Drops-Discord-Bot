using System.Diagnostics.CodeAnalysis;

namespace TwitchDropsDiscordBot.Persistence;

/// <summary>
/// Implementation of Alert History Repository.
/// Performs file-related actions for recording or retrieving alert history.
/// This may be converted to a SQLite database in-future.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Cannot unit test file-based persistence.")]
public sealed class AlertHistoryFileRepository
{
    private const string FilePath = "AlertHistory.txt";

    /// <summary>
    /// Ctor.
    /// </summary>
    public AlertHistoryFileRepository()
    {
        EnsureCreated();
    }

    /// <summary>
    /// Opens a StreamReader for the AlertHistory file.
    /// </summary>
    /// <returns>Instantiated stream.</returns>
    public StreamReader OpenReadStream()
    {
        return new StreamReader(FilePath);
    }

    /// <summary>
    /// Appends the supplied lines to the end of the file.
    /// </summary>
    /// <param name="lines"></param>
    public async Task AppendLineAsync(IEnumerable<string> lines)
    {
        await File.AppendAllLinesAsync(FilePath, lines);
    }

    private static void EnsureCreated()
    {
        if (!File.Exists(FilePath))
        {
            using (_ = File.CreateText(FilePath)) { }
        }
    }
}
