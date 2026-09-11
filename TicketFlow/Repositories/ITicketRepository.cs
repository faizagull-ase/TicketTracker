using TicketFlow.Models;

namespace TicketFlow.Repositories;

/// <summary>
/// Storage contract for tickets. Command-handling code depends only on this
/// interface, never on a concrete storage mechanism - so the in-memory
/// implementation used through Day 3 can be swapped for a file- or
/// database-backed one later without touching any calling code.
/// </summary>
public interface ITicketRepository
{
    Ticket Add(Ticket ticket);

    Ticket? GetById(Guid id);

    IReadOnlyList<Ticket> GetAll();

    /// <summary>Replaces an existing ticket with an updated copy. Throws if no ticket with that id exists.</summary>
    Ticket Update(Ticket ticket);
}
