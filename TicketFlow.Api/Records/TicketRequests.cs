namespace TicketFlow.Api.Records;

public record CreateTicketRequest(string? Title, string? Description, string? Priority, string? AssignedTo);
public record UpdateTicketRequest(string? Title, string? Description, string? Priority);
public record ChangeStatusRequest(string? Status);
public record AssignTicketRequest(string? Username);
