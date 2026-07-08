using System.Collections.Immutable;
using DiscordLab.Bot.API.Extensions;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using NorthwoodLib.Pools;

namespace LogAssistant;

public record struct LogEntry
{
    private static List<LogEntry> Entries { get; } = new();

    private static long Window { get; } = TimeSpan.FromSeconds(30).Ticks;
    
    private LogEntry(string message, string player, long ticks, string? relatedPlayer = null) {
        Message = message;
        PlayerId = player;
        Timestamp = new(ticks, DateTimeKind.Utc);
        Unix = new DateTimeOffset(Timestamp).ToUnixTimeSeconds();
        RelatedPlayerId = relatedPlayer;

        LogEntry entry = this;
        Entries.Add(entry);
        Timing.CallDelayed(600, () => Entries.Remove(entry));
    }
    
    private string Message { get; }
    
    public string PlayerId { get; }

    private DateTime Timestamp { get; }

    private long Unix { get; }

    public string? RelatedPlayerId { get; }

    public string BaseMessage => $"- <t:{Unix}:T> - {Message}";
    
    public LogEntry CreateForRelated()
    {
        LogEntry entry = new(Message, RelatedPlayerId!, Timestamp.Ticks, PlayerId);
        return entry;
    }

    public bool InWindow(LogEntry otherEntry)
    {
        long diff = Math.Abs(otherEntry.Timestamp.Ticks - Timestamp.Ticks);

        return diff <= Window;
    }

    public override string ToString() => $"{Timestamp.ToLongDateString()} {PlayerId} {Message}";

    public static LogEntry Create(string message, string player, string? relatedPlayer = null, DateTime? date = null)
    {
        date ??= DateTime.UtcNow;

        return new(message, player, date.Value.Ticks, relatedPlayer);
    }

    public static (IEnumerable<LogEntry> player, IEnumerable<LogEntry> others) GetEntries(string playerId)
    {
        ILookup<string, LogEntry> lookup = Entries.ToLookup(entry => entry.PlayerId);

        if (!lookup.Contains(playerId))
            return (Array.Empty<LogEntry>(), Array.Empty<LogEntry>());
        
        IEnumerable<LogEntry> playerEntries = lookup[playerId].ToArray();
        List<LogEntry> otherEntries = new();
        
        foreach (LogEntry playerEntry in playerEntries)
        {
            string? related = playerEntry.RelatedPlayerId;
            
            if (related == null)
                continue;

            if (!lookup.Contains(related))
                continue;

            // We don't want to make it so there is duplicates.
            if (otherEntries.Any(entry => entry.PlayerId == playerEntry.RelatedPlayerId))
                continue;
            
            otherEntries.AddRange(lookup[related]);
        }

        return (playerEntries.OrderBy(entry => entry.Timestamp.Ticks), otherEntries.OrderBy(entry => entry.Timestamp.Ticks));
    }
}