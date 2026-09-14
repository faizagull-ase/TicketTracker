using TicketFlow.Models;

namespace TicketFlow.Services;

public record TicketStats(
    int TotalCount,
    IReadOnlyDictionary<TicketStatus, int> ByStatus,
    IReadOnlyDictionary<TicketPriority, int> ByPriority,
    Ticket? OldestOpenTicket);
