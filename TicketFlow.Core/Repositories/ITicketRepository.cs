using TicketFlow.Models;

namespace TicketFlow.Repositories;

/// <summary>
/// Storage contract for tickets. Command-handling code depends only on this
/// interface, never on a concrete storage mechanism - so the file-backed
/// implementation can be swapped (e.g. for a database) without touching any
/// calling code. All I/O is asynchronous so a slow disk never blocks the
/// console loop.
/// </summary>
public interface ITicketRepository
{
    /// <summary>Adds a ticket and assigns it the next available id, ignoring whatever <see cref="Ticket.Id"/> it was created with.</summary>
    Task<Ticket> AddAsync(Ticket ticket);

    Task<Ticket?> GetByIdAsync(int id);

    Task<IReadOnlyList<Ticket>> GetAllAsync();

    /// <summary>Replaces an existing ticket with an updated copy. Throws if no ticket with that id exists.</summary>
    Task<Ticket> UpdateAsync(Ticket ticket);
}
