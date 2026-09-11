using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TicketFlow.Models;
using TicketFlow.Services;

namespace TicketFlow.Cli;

/// <summary>All console rendering (banner, tables, colors, messages) lives here, kept separate from command/business logic.</summary>
public class ConsoleUi
{
    public void ShowBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        DrawBoxTop(56);
        DrawBoxCenter("T I C K E T F L O W", 56);
        DrawBoxCenter("Support Ticket Tracker", 56);
        DrawBoxBottom(56);
        Console.ResetColor();
        Console.WriteLine();
        Info("Type 'help' to see available commands, or 'exit' to quit.");
        Console.WriteLine();
    }

    public void ShowHelp()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  COMMANDS");
        Console.ResetColor();

        WriteHelpLine("add --title \"...\" [--description \"...\"] [--priority Low|Medium|High|Critical] [--assignee name]",
            "Create a ticket (defaults to Medium priority, Open status).");
        WriteHelpLine("list [--status ...] [--priority ...] [--assignee ...] [--json]",
            "List tickets, newest first, with optional filters.");
        WriteHelpLine("status <id> <NewStatus>",
            "Change a ticket's status (Open, InProgress, Resolved, Closed).");
        WriteHelpLine("assign <id> <username>",
            "Set (or change) a ticket's assignee.");
        WriteHelpLine("search <text>",
            "Find tickets whose title or description contains the text.");
        WriteHelpLine("stats [--json]",
            "Counts by status and priority, plus the oldest open ticket.");
        WriteHelpLine("help", "Show this help screen.");
        WriteHelpLine("exit", "Quit TicketFlow.");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Tip: <id> accepts the full ticket id or any unambiguous prefix of it.");
        Console.WriteLine("  Note: data is in-memory only for now and resets each run (Day 4 adds persistence).");
        Console.ResetColor();
        Console.WriteLine();
    }

    public void Prompt() => Console.Write("ticketflow> ");

    public void Success(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [OK] {message}");
        Console.ResetColor();
    }

    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [!] {message}");
        Console.ResetColor();
    }

    public void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }

    public void RenderTicket(Ticket ticket)
    {
        Console.WriteLine($"    id       : {ticket.Id}");
        Console.WriteLine($"    title    : {ticket.Title}");

        if (!string.IsNullOrEmpty(ticket.Description))
        {
            Console.WriteLine($"    desc     : {ticket.Description}");
        }

        Console.Write("    priority : ");
        WriteColored(ticket.Priority.ToString(), PriorityColor(ticket.Priority));
        Console.Write("    status   : ");
        WriteColored(ticket.Status.ToString(), StatusColor(ticket.Status));
        Console.WriteLine($"    assignee : {ticket.AssignedTo ?? "(unassigned)"}");
        Console.WriteLine($"    created  : {ticket.CreatedAt.LocalDateTime:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"    updated  : {ticket.UpdatedAt.LocalDateTime:yyyy-MM-dd HH:mm}");
        Console.WriteLine();
    }

    public void RenderTicketsTable(IReadOnlyList<Ticket> tickets)
    {
        if (tickets.Count == 0)
        {
            Info("No tickets found.");
            return;
        }

        string[] headers = { "ID", "TITLE", "PRIORITY", "STATUS", "ASSIGNEE", "CREATED" };

        var rows = tickets
            .Select(t => new[]
            {
                t.ShortId,
                Truncate(t.Title, 32),
                t.Priority.ToString(),
                t.Status.ToString(),
                t.AssignedTo ?? "-",
                t.CreatedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        RenderTable(headers, rows, (row, col) => col switch
        {
            2 => PriorityColor(tickets[row].Priority),
            3 => StatusColor(tickets[row].Status),
            _ => null
        });

        Console.WriteLine();
        Info($"{tickets.Count} ticket(s).");
    }

    public void RenderStats(TicketStats stats)
    {
        Console.WriteLine();

        if (stats.TotalCount == 0)
        {
            Info("No tickets yet.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  TICKET STATS - {stats.TotalCount} total");
        Console.ResetColor();
        Console.WriteLine();

        var statusRows = stats.ByStatus
            .Select(kv => new[] { kv.Key.ToString(), kv.Value.ToString() })
            .ToList();
        RenderTable(new[] { "STATUS", "COUNT" }, statusRows,
            (row, col) => col == 0 ? StatusColor(stats.ByStatus.Keys.ElementAt(row)) : null);

        Console.WriteLine();

        var priorityRows = stats.ByPriority
            .Select(kv => new[] { kv.Key.ToString(), kv.Value.ToString() })
            .ToList();
        RenderTable(new[] { "PRIORITY", "COUNT" }, priorityRows,
            (row, col) => col == 0 ? PriorityColor(stats.ByPriority.Keys.ElementAt(row)) : null);

        Console.WriteLine();

        if (stats.OldestOpenTicket is { } oldest)
        {
            Console.WriteLine($"  Oldest open ticket : {oldest.ShortId} - \"{Truncate(oldest.Title, 40)}\" (opened {oldest.CreatedAt.LocalDateTime:yyyy-MM-dd HH:mm})");
        }
        else
        {
            Console.WriteLine("  Oldest open ticket : none - nothing is Open right now.");
        }

        Console.WriteLine();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public void RenderTicketsJson(IReadOnlyList<Ticket> tickets) =>
        Console.WriteLine(JsonSerializer.Serialize(tickets, JsonOptions));

    public void RenderStatsJson(TicketStats stats) =>
        Console.WriteLine(JsonSerializer.Serialize(stats, JsonOptions));

    private static void WriteHelpLine(string usage, string description)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"  {usage}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"      {description}");
        Console.ResetColor();
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..(maxLength - 1)] + "…";

    private static ConsoleColor PriorityColor(TicketPriority priority) => priority switch
    {
        TicketPriority.Low => ConsoleColor.Gray,
        TicketPriority.Medium => ConsoleColor.Cyan,
        TicketPriority.High => ConsoleColor.Yellow,
        TicketPriority.Critical => ConsoleColor.Red,
        _ => ConsoleColor.White
    };

    private static ConsoleColor StatusColor(TicketStatus status) => status switch
    {
        TicketStatus.Open => ConsoleColor.Green,
        TicketStatus.InProgress => ConsoleColor.Yellow,
        TicketStatus.Resolved => ConsoleColor.Blue,
        TicketStatus.Closed => ConsoleColor.DarkGray,
        _ => ConsoleColor.White
    };

    // ---- box / table drawing --------------------------------------------

    private static void DrawBoxTop(int width) => Console.WriteLine("  ╔" + new string('═', width) + "╗");

    private static void DrawBoxBottom(int width) => Console.WriteLine("  ╚" + new string('═', width) + "╝");

    private static void DrawBoxCenter(string text, int width)
    {
        int pad = Math.Max(0, width - text.Length);
        int left = pad / 2;
        int right = pad - left;
        Console.WriteLine("  ║" + new string(' ', left) + text + new string(' ', right) + "║");
    }

    private static void RenderTable(string[] headers, IReadOnlyList<string[]> rows, Func<int, int, ConsoleColor?>? cellColor = null)
    {
        int columns = headers.Length;
        var widths = new int[columns];

        for (int c = 0; c < columns; c++)
        {
            widths[c] = headers[c].Length;
        }

        foreach (string[] row in rows)
        {
            for (int c = 0; c < columns; c++)
            {
                widths[c] = Math.Max(widths[c], row[c].Length);
            }
        }

        void WriteSeparator(char left, char mid, char right)
        {
            var sb = new StringBuilder("  ").Append(left);

            for (int c = 0; c < columns; c++)
            {
                sb.Append(new string('═', widths[c] + 2));
                sb.Append(c == columns - 1 ? right : mid);
            }

            Console.WriteLine(sb.ToString());
        }

        void WriteRow(string[] cells, int rowIndex, bool isHeader)
        {
            Console.Write("  ║");

            for (int c = 0; c < columns; c++)
            {
                Console.Write(' ');

                ConsoleColor? color = isHeader ? ConsoleColor.Cyan : cellColor?.Invoke(rowIndex, c);

                if (color.HasValue)
                {
                    Console.ForegroundColor = color.Value;
                }

                Console.Write(cells[c].PadRight(widths[c]));

                if (color.HasValue)
                {
                    Console.ResetColor();
                }

                Console.Write(" ║");
            }

            Console.WriteLine();
        }

        WriteSeparator('╔', '╦', '╗');
        WriteRow(headers, -1, isHeader: true);
        WriteSeparator('╠', '╬', '╣');

        for (int r = 0; r < rows.Count; r++)
        {
            WriteRow(rows[r], r, isHeader: false);
        }

        WriteSeparator('╚', '╩', '╝');
    }
}
