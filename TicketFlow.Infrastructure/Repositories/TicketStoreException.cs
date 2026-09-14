namespace TicketFlow.Repositories;

/// <summary>
/// Base type for tickets.json read/write failures that the CLI shows as a single,
/// user-facing message (corrupt file, disk full, permission denied, etc.) instead of
/// leaking a raw <see cref="IOException"/> or <see cref="System.Text.Json.JsonException"/>.
/// Concrete subclasses (<see cref="TicketStoreCorruptException"/>, <see cref="TicketStoreIOException"/>)
/// each supply the recovery hint appropriate to their failure mode.
/// </summary>
public abstract class TicketStoreException : Exception
{
    public string FilePath { get; }

    /// <summary>A short, failure-specific suggestion shown alongside the message.</summary>
    public abstract string RecoveryHint { get; }

    protected TicketStoreException(string filePath, string message)
        : base(message)
    {
        FilePath = filePath;
    }

    protected TicketStoreException(string filePath, string message, Exception innerException)
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}
