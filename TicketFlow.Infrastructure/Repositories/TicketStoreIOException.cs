namespace TicketFlow.Repositories;

/// <summary>
/// tickets.json could not be read or written because of a disk/OS-level failure
/// (permission denied, disk full, file locked by another process, etc.).
/// </summary>
public sealed class TicketStoreIOException : TicketStoreException
{
    public override string RecoveryHint => "Check disk space and file permissions, then try again.";

    public TicketStoreIOException(string filePath, string message, Exception innerException)
        : base(filePath, message, innerException)
    {
    }
}
