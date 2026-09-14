using TicketFlow.Models;
using TicketFlow.Repositories;
using TicketFlow.Services;

namespace TicketFlow.Cli;

/// <summary>
/// Dispatches a tokenized command to the right <see cref="TicketService"/> call and
/// renders the result (or a friendly error) through <see cref="ConsoleUi"/>.
/// </summary>
public class CommandRouter
{
    private readonly TicketService _service;
    private readonly ConsoleUi _ui;

    public CommandRouter(TicketService service, ConsoleUi ui)
    {
        _service = service;
        _ui = ui;
    }

    public async Task ExecuteAsync(IReadOnlyList<string> tokens)
    {
        ParsedCommand command;

        try
        {
            command = CommandParser.Parse(tokens);
        }
        
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            _ui.Error(ex.Message);
            return;
        }

        try
        {
            switch (command.Name)
            {
                case "add":
                    await HandleAddAsync(command);
                    break;
                case "list":
                    await HandleListAsync(command);
                    break;
                case "status":
                    await HandleStatusAsync(command);
                    break;
                case "assign":
                    await HandleAssignAsync(command);
                    break;
                case "search":
                    await HandleSearchAsync(command);
                    break;
                case "stats":
                    await HandleStatsAsync(command);
                    break;
                case "help":
                    _ui.ShowHelp();
                    break;
                default:
                    _ui.Error($"Unknown command '{command.Name}'. Type 'help' to see available commands.");
                    break;
            }
        }
        catch (TicketStoreException ex)
        {
            _ui.Error($"{ex.Message} {ex.RecoveryHint}");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            _ui.Error(ex.Message);
        }
    }

    private async Task HandleAddAsync(ParsedCommand command)
    {
        Ticket ticket = await _service.AddTicketAsync(
            command.Option("title"),
            command.Option("description"),
            command.Option("priority"),
            command.Option("assignee"));

        _ui.Success($"Created ticket {ticket.ShortId} - \"{ticket.Title}\"");
        _ui.RenderTicket(ticket);
    }

    private async Task HandleListAsync(ParsedCommand command)
    {
        IReadOnlyList<Ticket> tickets = await _service.ListTicketsAsync(
            command.Option("status"),
            command.Option("priority"),
            command.Option("assignee"));

        if (command.HasFlag("json"))
        {
            _ui.RenderTicketsJson(tickets);
        }
        else
        {
            _ui.RenderTicketsTable(tickets);
        }
    }

    private async Task HandleStatusAsync(ParsedCommand command)
    {
        if (command.Positional.Count < 2)
        {
            throw new ArgumentException("Usage: status <id> <NewStatus>");
        }

        Ticket updated = await _service.ChangeStatusAsync(command.Positional[0], command.Positional[1]);
        _ui.Success($"Ticket {updated.ShortId} status set to {updated.Status}.");
        _ui.RenderTicket(updated);
    }

    private async Task HandleAssignAsync(ParsedCommand command)
    {
        if (command.Positional.Count < 2)
        {
            throw new ArgumentException("Usage: assign <id> <username>");
        }

        Ticket updated = await _service.AssignTicketAsync(command.Positional[0], command.Positional[1]);
        _ui.Success($"Ticket {updated.ShortId} assigned to {updated.AssignedTo}.");
        _ui.RenderTicket(updated);
    }

    private async Task HandleSearchAsync(ParsedCommand command)
    {
        if (command.Positional.Count == 0)
        {
            throw new ArgumentException("Usage: search <text>");
        }

        string text = string.Join(' ', command.Positional);
        IReadOnlyList<Ticket> tickets = await _service.SearchTicketsAsync(text);
        _ui.RenderTicketsTable(tickets);
    }

    private async Task HandleStatsAsync(ParsedCommand command)
    {
        TicketStats stats = await _service.GetStatsAsync();

        if (command.HasFlag("json"))
        {
            _ui.RenderStatsJson(stats);
        }
        else
        {
            _ui.RenderStats(stats);
        }
    }
}
