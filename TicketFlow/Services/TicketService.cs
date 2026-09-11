using TicketFlow.Models;
using TicketFlow.Repositories;

namespace TicketFlow.Services;

/// <summary>
/// Application logic for creating, filtering, and updating tickets. Composes
/// an <see cref="ITicketRepository"/> (has-a) rather than being one itself,
/// so the storage mechanism can change without this class changing.
/// </summary>
public class TicketService
{
    private readonly ITicketRepository _repository;

    public TicketService(ITicketRepository repository)
    {
        _repository = repository;
    }

    public Ticket AddTicket(string? title, string? description, string? priorityRaw, string? assignee)
    {
        TicketPriority priority = ParsePriority(priorityRaw, TicketPriority.Medium);
        Ticket ticket = Ticket.Create(title, description, priority, assignee);
        return _repository.Add(ticket);
    }

    public IReadOnlyList<Ticket> ListTickets(string? statusRaw, string? priorityRaw, string? assigneeRaw)
    {
        IEnumerable<Ticket> query = _repository.GetAll();

        if (!string.IsNullOrWhiteSpace(statusRaw))
        {
            TicketStatus status = ParseStatus(statusRaw);
            query = query.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(priorityRaw))
        {
            TicketPriority priority = ParsePriority(priorityRaw, TicketPriority.Medium);
            query = query.Where(t => t.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(assigneeRaw))
        {
            string assignee = assigneeRaw.Trim();
            query = query.Where(t => t.AssignedTo is not null
                && t.AssignedTo.Equals(assignee, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderByDescending(t => t.CreatedAt).ToList();
    }

    public IReadOnlyList<Ticket> SearchTickets(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Search text is required.", nameof(text));
        }

        string needle = text.Trim();

        return _repository.GetAll()
            .Where(t => t.Title.Contains(needle, StringComparison.OrdinalIgnoreCase)
                || (t.Description?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }

    public Ticket ChangeStatus(string idRaw, string newStatusRaw)
    {
        TicketStatus newStatus = ParseStatus(newStatusRaw);
        Ticket ticket = ResolveTicket(idRaw);
        Ticket updated = ticket with { Status = newStatus, UpdatedAt = DateTimeOffset.UtcNow };
        return _repository.Update(updated);
    }

    public Ticket AssignTicket(string idRaw, string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.", nameof(username));
        }

        Ticket ticket = ResolveTicket(idRaw);
        Ticket updated = ticket with { AssignedTo = username.Trim(), UpdatedAt = DateTimeOffset.UtcNow };
        return _repository.Update(updated);
    }

    public TicketStats GetStats()
    {
        IReadOnlyList<Ticket> all = _repository.GetAll();

        Dictionary<TicketStatus, int> byStatus = Enum.GetValues<TicketStatus>()
            .ToDictionary(status => status, status => all.Count(t => t.Status == status));

        Dictionary<TicketPriority, int> byPriority = Enum.GetValues<TicketPriority>()
            .ToDictionary(priority => priority, priority => all.Count(t => t.Priority == priority));

        Ticket? oldestOpen = all
            .Where(t => t.Status == TicketStatus.Open)
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefault();

        return new TicketStats(all.Count, byStatus, byPriority, oldestOpen);
    }

    /// <summary>
    /// Finds a ticket by full id or by an unambiguous id prefix, so a user
    /// doesn't have to retype a full GUID at the console.
    /// </summary>
    private Ticket ResolveTicket(string? idRaw)
    {
        if (string.IsNullOrWhiteSpace(idRaw))
        {
            throw new ArgumentException("Ticket id is required.", nameof(idRaw));
        }

        string id = idRaw.Trim();

        if (Guid.TryParse(id, out Guid exact))
        {
            return _repository.GetById(exact)
                ?? throw new InvalidOperationException($"No ticket found with id '{id}'.");
        }

        List<Ticket> matches = _repository.GetAll()
            .Where(t => t.Id.ToString().StartsWith(id, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            0 => throw new InvalidOperationException($"No ticket found with id '{id}'."),
            1 => matches[0],
            _ => throw new InvalidOperationException(
                $"Id '{id}' matches {matches.Count} tickets; use more characters to disambiguate.")
        };
    }

    private static TicketPriority ParsePriority(string? raw, TicketPriority defaultValue)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        if (!Enum.TryParse(raw, ignoreCase: true, out TicketPriority parsed))
        {
            throw new ArgumentException(
                $"Invalid priority '{raw}'. Valid values: {string.Join(", ", Enum.GetNames<TicketPriority>())}.");
        }

        return parsed;
    }

    private static TicketStatus ParseStatus(string? raw)
    {
        if (!Enum.TryParse(raw, ignoreCase: true, out TicketStatus parsed))
        {
            throw new ArgumentException(
                $"Invalid status '{raw}'. Valid values: {string.Join(", ", Enum.GetNames<TicketStatus>())}.");
        }

        return parsed;
    }
}
