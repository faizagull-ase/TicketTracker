namespace TicketFlow.Repositories;

/// <summary>
/// tickets.json exists but its contents are unusable - invalid JSON, or an entry
/// missing a required field (id/title).
/// </summary>
public sealed class TicketStoreCorruptException : TicketStoreException
{
    public override string RecoveryHint => "Fix or delete the file, then restart TicketFlow.";

    public TicketStoreCorruptException(string filePath, string message)
        : base(filePath, message)
    {
    }

    public TicketStoreCorruptException(string filePath, string message, Exception innerException)
        : base(filePath, message, innerException)
    {
    }
}
