namespace TicketFlow.Cli;

/// <summary>A raw command line broken into a command name, --flag options, and positional arguments.</summary>
public record ParsedCommand(
    string Name,
    IReadOnlyDictionary<string, string> Options,
    IReadOnlyList<string> Positional)
{
    public bool HasFlag(string name) => Options.ContainsKey(name);

    public string? Option(string name) => Options.TryGetValue(name, out string? value) ? value : null;
}
