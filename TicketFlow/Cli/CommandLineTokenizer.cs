using System.Text;

namespace TicketFlow.Cli;

/// <summary>
/// Splits one line of typed REPL input into tokens, respecting double-quoted
/// sections so <c>--title "Login page is broken"</c> stays one token.
/// (When commands arrive as process <c>args</c>, the shell has already done
/// this splitting, so this tokenizer is only needed for the REPL.)
/// </summary>
public static class CommandLineTokenizer
{
    public static string[] Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(c);
        }

        if (inQuotes)
        {
            throw new FormatException("Unmatched \" in input - every opening quote needs a closing quote.");
        }

        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }

        return tokens.ToArray();
    }
}
