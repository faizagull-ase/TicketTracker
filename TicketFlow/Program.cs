using TicketFlow.Cli;
using TicketFlow.Repositories;
using TicketFlow.Services;

string dataFilePath = Path.Combine(AppContext.BaseDirectory, "tickets.json");
var repository = new JsonFileTicketRepository(dataFilePath);
var service = new TicketService(repository);
var ui = new ConsoleUi();
var router = new CommandRouter(service, ui);

try
{
    await repository.InitializeAsync();
}
catch (TicketStoreException ex)
{
    ui.Error($"{ex.Message} {ex.RecoveryHint}");
    return 1;
}

// Single-shot mode: `dotnet run -- add --title "..."`. Tickets persist to
// tickets.json next to the built exe, so this works across separate runs.
if (args.Length > 0)
{
    await router.ExecuteAsync(args);
    return 0;
}

// No args (e.g. F5 in Visual Studio): interactive session.
ui.(ShowBanner);

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

    await router.ExecuteAsync(tokens);
}

ui.Info("Goodbye!");
return 0;
