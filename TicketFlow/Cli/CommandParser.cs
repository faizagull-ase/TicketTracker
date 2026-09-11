namespace TicketFlow.Cli;

/// <summary>Turns already-tokenized input (e.g. ["add", "--title", "Fix it", "--priority", "High"]) into a <see cref="ParsedCommand"/>.</summary>
public static class CommandParser
{
    public static ParsedCommand Parse(IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new ArgumentException("No command was entered. Type 'help' to see available commands.");
        }

        string name = tokens[0].ToLowerInvariant();
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var positional = new List<string>();

        int i = 1;
        while (i < tokens.Count)
        {
            string token = tokens[i];

            if (token.StartsWith("--", StringComparison.Ordinal))
            {
                string key = token[2..];

                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("Found '--' with no flag name after it.");
                }

                bool hasValue = i + 1 < tokens.Count && !tokens[i + 1].StartsWith("--", StringComparison.Ordinal);
                options[key] = hasValue ? tokens[++i] : "true";
            }
            else
            {
                positional.Add(token);
            }

            i++;
        }

        return new ParsedCommand(name, options, positional);
    }
}
