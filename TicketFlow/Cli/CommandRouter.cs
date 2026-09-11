using TicketFlow.Models;
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

    public void Execute(IReadOnlyList<string> tokens)
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
                    HandleAdd(command);
                    break;
                case "list":
                    HandleList(command);
                    break;
                case "status":
                    HandleStatus(command);
                    break;
                case "assign":
                    HandleAssign(command);
                    break;
                case "search":
                    HandleSearch(command);
                    break;
                case "stats":
                    HandleStats(command);
                    break;
                case "help":
                    _ui.ShowHelp();
                    break;
                default:
                    _ui.Error($"Unknown command '{command.Name}'. Type 'help' to see available commands.");
                    break;
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            _ui.Error(ex.Message);
        }
    }

    private void HandleAdd(ParsedCommand command)
    {
        Ticket ticket = _service.AddTicket(
            command.Option("title"),
            command.Option("description"),
            command.Option("priority"),
            command.Option("assignee"));

        _ui.Success($"Created ticket {ticket.ShortId} - \"{ticket.Title}\"");
        _ui.RenderTicket(ticket);
    }

    private void HandleList(ParsedCommand command)
    {
        IReadOnlyList<Ticket> tickets = _service.ListTickets(
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

    private void HandleStatus(ParsedCommand command)
    {
        if (command.Positional.Count < 2)
        {
            throw new ArgumentException("Usage: status <id> <NewStatus>");
        }

        Ticket updated = _service.ChangeStatus(command.Positional[0], command.Positional[1]);
        _ui.Success($"Ticket {updated.ShortId} status set to {updated.Status}.");
        _ui.RenderTicket(updated);
    }

    private void HandleAssign(ParsedCommand command)
    {
        if (command.Positional.Count < 2)
        {
            throw new ArgumentException("Usage: assign <id> <username>");
        }

        Ticket updated = _service.AssignTicket(command.Positional[0], command.Positional[1]);
        _ui.Success($"Ticket {updated.ShortId} assigned to {updated.AssignedTo}.");
        _ui.RenderTicket(updated);
    }

    private void HandleSearch(ParsedCommand command)
    {
        if (command.Positional.Count == 0)
        {
            throw new ArgumentException("Usage: search <text>");
        }

        string text = string.Join(' ', command.Positional);
        _ui.RenderTicketsTable(_service.SearchTickets(text));
    }

    private void HandleStats(ParsedCommand command)
    {
        TicketStats stats = _service.GetStats();

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
