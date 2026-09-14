using Microsoft.AspNetCore.Mvc;
using TicketFlow.Api.Records;
using TicketFlow.Models;
using TicketFlow.Repositories;
using TicketFlow.Services;

namespace TicketFlow.Api.Controllers;

/// <summary>
/// HTTP surface for tickets. All validation and state changes happen in
/// <see cref="TicketService"/> - this class only translates requests/responses
/// and maps the service's exceptions onto RESTful status codes.
/// </summary>
[ApiController]
[Route("tickets")]
public class TicketsController : ControllerBase
{
    private readonly TicketService _service;

    public TicketsController(TicketService service)
    {
        _service = service;
    }

    [HttpPost]
    public Task<IActionResult> Create(CreateTicketRequest request) =>
        RunAsync(
            () => _service.AddTicketAsync(request.Title, request.Description, request.Priority, request.AssignedTo),
            ticket => CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket));

    [HttpGet]
    public Task<IActionResult> List([FromQuery] string? status, [FromQuery] string? priority, [FromQuery] string? assignee) =>
        RunAsync(() => _service.ListTicketsAsync(status, priority, assignee));

    [HttpGet("stats")]
    public Task<IActionResult> GetStats() =>
        RunAsync(() => _service.GetStatsAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        Ticket? ticket = await _service.GetTicketAsync(id);
        return ticket is not null ? Ok(ticket) : NotFound(new { error = $"No ticket found with id '{id}'." });
    }

    [HttpPut("{id:int}")]
    public Task<IActionResult> Update(int id, UpdateTicketRequest request) =>
        RunAsync(
            () => _service.UpdateTicketAsync(id.ToString(), request.Title, request.Description, request.Priority),
            _ => NoContent());

    [HttpPatch("{id:int}/status")]
    public Task<IActionResult> ChangeStatus(int id, ChangeStatusRequest request) =>
        RunAsync(() => _service.ChangeStatusAsync(id.ToString(), request.Status));

    [HttpPatch("{id:int}/assignee")]
    public Task<IActionResult> AssignTicket(int id, AssignTicketRequest request) =>
        RunAsync(() => _service.AssignTicketAsync(id.ToString(), request.Username));

    private Task<IActionResult> RunAsync<T>(Func<Task<T>> operation) => RunAsync(operation, value => Ok(value));

    /// <summary>Runs a <see cref="TicketService"/> call and maps its exceptions to HTTP status codes: 400 for bad input, 404 for "not found", 500 for a storage failure.</summary>
    private async Task<IActionResult> RunAsync<T>(Func<Task<T>> operation, Func<T, IActionResult> onSuccess)
    {
        try
        {
            return onSuccess(await operation());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (TicketStoreException ex)
        {
            return Problem($"{ex.Message} {ex.RecoveryHint}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
