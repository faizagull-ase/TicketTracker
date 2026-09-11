using TicketFlow.Models;

namespace TicketFlow.Repositories;

/// <summary>
/// Keeps tickets in memory for the lifetime of the process. This is the
/// Day 1-3 storage; Day 4 introduces async persistence to tickets.json
/// behind this same <see cref="ITicketRepository"/> contract.
/// </summary>
public class InMemoryTicketRepository : ITicketRepository
{
    private readonly Dictionary<Guid, Ticket> _tickets = new();

    public Ticket Add(Ticket ticket)
    {
        _tickets[ticket.Id] = ticket;
        return ticket;
    }

    public Ticket? GetById(Guid id) => _tickets.TryGetValue(id, out Ticket? ticket) ? ticket : null;

    public IReadOnlyList<Ticket> GetAll() => _tickets.Values.ToList();

    public Ticket Update(Ticket ticket)
    {
        if (!_tickets.ContainsKey(ticket.Id))
        {
            throw new InvalidOperationException($"Cannot update ticket '{ticket.Id}' because it does not exist.");
        }

        _tickets[ticket.Id] = ticket;
        return ticket;
    }
}
