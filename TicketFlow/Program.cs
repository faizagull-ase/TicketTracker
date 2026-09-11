using TicketFlow.Cli;
using TicketFlow.Repositories;
using TicketFlow.Services;

ITicketRepository repository = new InMemoryTicketRepository();
var service = new TicketService(repository);
var ui = new ConsoleUi();
var router = new CommandRouter(service, ui);

// Single-shot mode: `dotnet run -- add --title "..."`. Data is in-memory only
// (no tickets.json until Day 4), so each process run starts empty again.
if (args.Length > 0)
{
    router.Execute(args);
    return;
}

// No args (e.g. F5 in Visual Studio): interactive session so add/list/status/
// assign/stats can all be demoed together against the same in-memory data.
ui.ShowBanner();

while (true)
{
    ui.Prompt();
    string? line = Console.ReadLine();

    if (line is null)
    {
        break;
    }

    line = line.Trim();

    if (line.Length == 0)
    {
        continue;
    }

    if (line.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        line.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    string[] tokens;

    try
    {
        tokens = CommandLineTokenizer.Tokenize(line);
    }
    catch (FormatException ex)
    {
        ui.Error(ex.Message);
        continue;
    }

    if (tokens.Length == 0)
    {
        continue;
    }

    router.Execute(tokens);
}

ui.Info("Goodbye!");
