using TicketFlow.Models;

namespace TicketFlow.Repositories;

/// <summary>
/// Keeps tickets in memory only, with no disk I/O. Useful as a fast test
/// double for <see cref="Services.TicketService"/>; the app itself uses
/// <see cref="JsonFileTicketRepository"/> so data survives a restart.
/// </summary>
public class InMemoryTicketRepository : ITicketRepository
{
    private readonly Dictionary<int, Ticket> _tickets = new();
    private int _nextId = 1;

    public Task<Ticket> AddAsync(Ticket ticket)
    {
        Ticket stored = ticket with { Id = _nextId++ };
        _tickets[stored.Id] = stored;
        return Task.FromResult(stored);
    }

    public Task<Ticket?> GetByIdAsync(int id) =>
        Task.FromResult(_tickets.TryGetValue(id, out Ticket? ticket) ? ticket : null);

    public Task<IReadOnlyList<Ticket>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Ticket>>(_tickets.Values.ToList());

    public Task<Ticket> UpdateAsync(Ticket ticket)
    {
        if (!_tickets.ContainsKey(ticket.Id))
        {
            throw new InvalidOperationException($"Cannot update ticket '{ticket.Id}' because it does not exist.");
        }

        _tickets[ticket.Id] = ticket;
        return Task.FromResult(ticket);
    }
}
