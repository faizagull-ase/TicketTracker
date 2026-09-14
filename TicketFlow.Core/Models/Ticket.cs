namespace TicketFlow.Models;

/// <summary>
/// An immutable support ticket. All state changes (status, assignment) go
/// through <c>with</c> expressions rather than in-place mutation, so every
/// version of a ticket that ever existed is a distinct, comparable value.
/// </summary>
public record Ticket(
    int Id,
    string Title,
    string? Description,
    TicketPriority Priority,
    TicketStatus Status,
    string? AssignedTo,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
    )

{
    public const int MaxTitleLength = 120;

    /// <summary>
    /// Creates a new, validated ticket in the <see cref="TicketStatus.Open"/> state.
    /// Validation lives here (not in the record constructor) so that <c>with</c>
    /// expressions used to update an existing ticket stay simple and don't
    /// re-run creation rules like "title is required". <see cref="Id"/> is left as
    /// 0 - a real, unique id is assigned by <see cref="Repositories.ITicketRepository.AddAsync"/>.
    /// </summary>
    public static Ticket Create(string? title, string? description, TicketPriority priority, string? assignedTo)
    {
        string trimmedTitle = (title ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(trimmedTitle))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (trimmedTitle.Length > MaxTitleLength)
        {
            throw new ArgumentException(
                $"Title must be {MaxTitleLength} characters or fewer (was {trimmedTitle.Length}).",
                nameof(title));
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        return new Ticket(
            Id: 0,
            Title: trimmedTitle,
            Description: string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Priority: priority,
            Status: TicketStatus.Open,
            AssignedTo: string.IsNullOrWhiteSpace(assignedTo) ? null : assignedTo.Trim(),
            CreatedAt: now,
            UpdatedAt: now);
    }

    public string ShortId => Id.ToString();
}
