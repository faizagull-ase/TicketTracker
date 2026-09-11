using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

while (true)
{
    ShowHeader();
    ShowMenu();

    Console.Write("  Select an option: ");
    string choice = Console.ReadLine() ?? "";

    Console.WriteLine();

    switch (choice)
    {
        case "1":
            AnalyzeInput();
            break;

        case "2":
            ShowHelp();
            break;

        case "3":
            ExitApplication();
            return;

        default:
            ShowError("Invalid option. Please select 1, 2, or 3.");
            Pause();
            break;
    }
}


// ==================================================
// MAIN MENU
// ==================================================

static void ShowMenu()
{
    Console.ForegroundColor = ConsoleColor.Cyan;

    Console.WriteLine("  ╔══════════════════════════════════════════╗");
    Console.WriteLine("  ║                 MAIN MENU                ║");
    Console.WriteLine("  ╠══════════════════════════════════════════╣");
    Console.WriteLine("  ║                                          ║");
    Console.WriteLine("  ║   [1]  Analyze Input                    ║");
    Console.WriteLine("  ║   [2]  Help                             ║");
    Console.WriteLine("  ║   [3]  Exit                             ║");
    Console.WriteLine("  ║                                          ║");
    Console.WriteLine("  ╚══════════════════════════════════════════╝");

    Console.ResetColor();
    Console.WriteLine();
}


// ==================================================
// APPLICATION HEADER
// ==================================================

static void ShowHeader()
{
    Console.Clear();

    Console.ForegroundColor = ConsoleColor.Cyan;

    Console.WriteLine();
    Console.WriteLine("  ╔══════════════════════════════════════════╗");
    Console.WriteLine("  ║                                          ║");
    Console.WriteLine("  ║       INPUT PARSER & SUMMARIZER         ║");
    Console.WriteLine("  ║                                          ║");
    Console.WriteLine("  ╚══════════════════════════════════════════╝");

    Console.ResetColor();

    Console.WriteLine();
    Console.WriteLine("  Analyze numbers or text and get useful");
    Console.WriteLine("  information about your input.");
    Console.WriteLine();
}


// ==================================================
// ANALYZE INPUT
// ==================================================

static void AnalyzeInput()
{
    Console.Clear();

    ShowSectionHeader("ANALYZE INPUT");

    // Level 2: "try again" loop lets the user analyze
    // multiple inputs without returning to the main menu.
    bool again = true;

    while (again)
    {
        Console.WriteLine("  Enter numbers separated by spaces");
        Console.WriteLine("  or enter a sentence/text.");
        Console.WriteLine();
        Console.WriteLine("  Examples:");
        Console.WriteLine("    10 20 30 40");
        Console.WriteLine("    10.5 20.75 -5 0");
        Console.WriteLine("    I am learning CSharp");
        Console.WriteLine();

        Console.Write("  Input: ");

        string input = Console.ReadLine() ?? "";

        // Allow user to return to the main menu
        if (input.Equals("back", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Allow user to exit from anywhere
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            ExitApplication();
            Environment.Exit(0);
        }

        // Empty input validation
        if (string.IsNullOrWhiteSpace(input))
        {
            ShowError("Input cannot be empty.");
            Pause();
            Console.Clear();
            ShowSectionHeader("ANALYZE INPUT");
            continue;
        }

        Console.WriteLine();

        // Detect numbers
        if (IsNumberInput(input))
        {
            ShowSuccess("Input detected as NUMBERS.");

            if (TryParseNumbers(input, out double[] numbers))
            {
                ShowParsedNumbers(numbers);
                SummarizeNumbers(numbers);
            }
            else
            {
                ShowError("Unable to parse the numbers.");
            }
        }
        else
        {
            ShowSuccess("Input detected as TEXT.");

            string[] words = ParseWords(input);

            ShowParsedWords(words);
            SummarizeText(words, input);
        }

        again = AskTryAgain();

        if (again)
        {
            Console.Clear();
            ShowSectionHeader("ANALYZE INPUT");
        }
    }

    Pause();
}


// ==================================================
// TRY AGAIN PROMPT
// ==================================================

static bool AskTryAgain()
{
    while (true)
    {
        Console.WriteLine();
        Console.Write("  Analyze another input? [Y/N]: ");

        string answer = (Console.ReadLine() ?? "").Trim();

        if (answer.Equals("y", StringComparison.OrdinalIgnoreCase) ||
            answer.Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (answer.Equals("n", StringComparison.OrdinalIgnoreCase) ||
            answer.Equals("no", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        ShowError("Please answer Y or N.");
    }
}


// ==================================================
// NUMBER TYPE CHECK
// ==================================================

static bool IsNumberInput(string input)
{
    string[] parts = input.Split(
        ' ',
        StringSplitOptions.RemoveEmptyEntries);

    // Level 2: explicit, culture-invariant number parsing
    // instead of relying on the machine's current culture.
    return parts.All(part => double.TryParse(
        part,
        NumberStyles.Float | NumberStyles.AllowThousands,
        CultureInfo.InvariantCulture,
        out _));
}


// ==================================================
// PARSE NUMBERS
// ==================================================

static bool TryParseNumbers(
    string input,
    out double[] numbers)
{
    string[] parts = input.Split(
        ' ',
        StringSplitOptions.RemoveEmptyEntries);

    numbers = new double[parts.Length];

    for (int i = 0; i < parts.Length; i++)
    {
        if (!double.TryParse(
            parts[i],
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture,
            out numbers[i]))
        {
            numbers = Array.Empty<double>();
            return false;
        }
    }

    return true;
}


// ==================================================
// PARSE TEXT
// ==================================================

static string[] ParseWords(string input)
{
    return input.Split(
        ' ',
        StringSplitOptions.RemoveEmptyEntries);
}


// ==================================================
// CLEAN WORD (strip surrounding punctuation)
// ==================================================

// Level 2: normalize punctuation so "Hello," and "Hello"
// (or "hello!" and "hello") are treated as the same word.
// Keeps internal characters like apostrophes/hyphens
// (e.g. "don't", "well-known") intact.
static string CleanWord(string word)
{
    return Regex.Replace(word, @"^[^\p{L}\p{N}]+|[^\p{L}\p{N}]+$", "");
}


// ==================================================
// SHOW PARSED NUMBERS (before summary)
// ==================================================

static void ShowParsedNumbers(double[] numbers)
{
    string formatted = string.Join(
        ", ",
        numbers.Select(n => n.ToString("0.##", CultureInfo.InvariantCulture)));

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine($"  Parsed input ({numbers.Length} number(s)):");
    Console.WriteLine($"    {formatted}");
    Console.ResetColor();
}


// ==================================================
// SHOW PARSED WORDS (before summary)
// ==================================================

static void ShowParsedWords(string[] words)
{
    string formatted = string.Join(
        " | ",
        words.Select((w, i) => $"{i + 1}:{w}"));

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine($"  Parsed input ({words.Length} token(s)):");
    Console.WriteLine($"    {formatted}");
    Console.ResetColor();
}


// ==================================================
// NUMBER SUMMARY
// ==================================================

static void SummarizeNumbers(double[] numbers)
{
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Yellow;

    Console.WriteLine("  ╔══════════════════════════════════════════╗");
    Console.WriteLine("  ║              NUMBER SUMMARY              ║");
    Console.WriteLine("  ╠══════════════════════════════════════════╣");

    Console.ResetColor();

    Console.WriteLine($"  ║  Count       : {numbers.Length,-22}║");
    Console.WriteLine($"  ║  Sum         : {numbers.Sum(),-22:F2}║");
    Console.WriteLine($"  ║  Average     : {numbers.Average(),-22:F2}║");
    Console.WriteLine($"  ║  Minimum     : {numbers.Min(),-22:F2}║");
    Console.WriteLine($"  ║  Maximum     : {numbers.Max(),-22:F2}║");

    Console.WriteLine("  ╠══════════════════════════════════════════╣");

    int positive = numbers.Count(n => n > 0);
    int negative = numbers.Count(n => n < 0);
    int zero = numbers.Count(n => n == 0);

    Console.WriteLine($"  ║  Positive    : {positive,-22}║");
    Console.WriteLine($"  ║  Negative    : {negative,-22}║");
    Console.WriteLine($"  ║  Zero        : {zero,-22}║");

    Console.WriteLine("  ╠══════════════════════════════════════════╣");

    int wholeNumbers = numbers.Count(n => n % 1 == 0);

    int evenNumbers = numbers.Count(
        n => n % 1 == 0 && n % 2 == 0);

    int oddNumbers = numbers.Count(
        n => n % 1 == 0 && n % 2 != 0);

    Console.WriteLine($"  ║  Whole       : {wholeNumbers,-22}║");
    Console.WriteLine($"  ║  Even        : {evenNumbers,-22}║");
    Console.WriteLine($"  ║  Odd         : {oddNumbers,-22}║");

    Console.WriteLine("  ╠══════════════════════════════════════════╣");

    // Level 2: "Duplicates" now clearly means the count of
    // extra (duplicate) occurrences, and we also list which
    // distinct values were actually duplicated.
    int uniqueNumbers = numbers.Distinct().Count();
    int duplicateOccurrences = numbers.Length - uniqueNumbers;

    var duplicateValues = numbers
        .GroupBy(n => n)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key)
        .OrderBy(n => n)
        .ToArray();

    Console.WriteLine($"  ║  Unique              : {uniqueNumbers,-14}║");
    Console.WriteLine($"  ║  Duplicate occur.    : {duplicateOccurrences,-14}║");

    string duplicateValuesText = duplicateValues.Length == 0
        ? "None"
        : string.Join(", ", duplicateValues.Select(v => v.ToString("0.##", CultureInfo.InvariantCulture)));

    Console.WriteLine($"  ║  Duplicate values    : {duplicateValuesText,-14}║");

    Console.WriteLine("  ╚══════════════════════════════════════════╝");

    Console.ResetColor();
}


// ==================================================
// TEXT SUMMARY
// ==================================================

static void SummarizeText(
    string[] words,
    string input)
{
    Console.WriteLine();

    int charactersWithoutSpaces = input.Count(
        c => !char.IsWhiteSpace(c));

    // Level 2: punctuation is stripped before comparing
    // words, so "Hello," and "hello!" count as the same word.
    string[] cleanedWords = words
        .Select(CleanWord)
        .Where(w => !string.IsNullOrEmpty(w))
        .ToArray();

    // Fall back to raw words if cleaning removed everything
    // (e.g. input made entirely of punctuation).
    string[] wordsForStats = cleanedWords.Length > 0 ? cleanedWords : words;

    int uniqueWords = wordsForStats
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Count();

    double averageWordLength = wordsForStats
        .Average(word => word.Length);

    string longestWord = wordsForStats
        .OrderByDescending(word => word.Length)
        .First();

    string shortestWord = wordsForStats
        .OrderBy(word => word.Length)
        .First();

    Console.ForegroundColor = ConsoleColor.Yellow;

    Console.WriteLine("  ╔══════════════════════════════════════════╗");
    Console.WriteLine("  ║                TEXT SUMMARY              ║");
    Console.WriteLine("  ╠══════════════════════════════════════════╣");

    Console.ResetColor();

    Console.WriteLine($"  ║  Words               : {words.Length,-15}║");
    Console.WriteLine($"  ║  Characters          : {input.Length,-15}║");
    Console.WriteLine($"  ║  Without spaces      : {charactersWithoutSpaces,-15}║");
    Console.WriteLine($"  ║  Unique words        : {uniqueWords,-15}║");
    Console.WriteLine($"  ║  Average word length : {averageWordLength,-15:F2}║");

    Console.WriteLine("  ╠══════════════════════════════════════════╣");

    Console.WriteLine($"  ║  Longest word  : {longestWord,-20}║");
    Console.WriteLine($"  ║  Shortest word : {shortestWord,-20}║");

    Console.WriteLine("  ╚══════════════════════════════════════════╝");

    Console.ResetColor();

    ShowWordFrequency(wordsForStats);
}


// ==================================================
// WORD FREQUENCY (Level 2 addition)
// ==================================================

static void ShowWordFrequency(string[] words)
{
    var frequency = words
        .GroupBy(w => w, StringComparer.OrdinalIgnoreCase)
        .OrderByDescending(g => g.Count())
        .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
        .Select(g => (Word: g.Key, Count: g.Count()))
        .ToList();

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Magenta;

    Console.WriteLine("  ╔══════════════════════════════════════════╗");
    Console.WriteLine("  ║              WORD FREQUENCY              ║");
    Console.WriteLine("  ╠══════════════════════════════════════════╣");

    Console.ResetColor();

    foreach (var (word, count) in frequency)
    {
        Console.WriteLine($"  ║  {word,-30}: {count,-9}║");
    }

    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("  ╚══════════════════════════════════════════╝");
    Console.ResetColor();
}


// ==================================================
// HELP
// ==================================================

static void ShowHelp()
{
    Console.Clear();

    ShowSectionHeader("HELP");

    Console.ForegroundColor = ConsoleColor.White;

    Console.WriteLine("  This application analyzes numbers and text.");
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("  NUMBER INPUT");
    Console.ResetColor();

    Console.WriteLine("  Example:");
    Console.WriteLine("    10 20 30 40");
    Console.WriteLine("    10.5 20.75 -5 0");
    Console.WriteLine("    (Uses invariant '.' as decimal separator)");
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("  NUMBER SUMMARY");
    Console.ResetColor();

    Console.WriteLine("  • Count");
    Console.WriteLine("  • Sum");
    Console.WriteLine("  • Average");
    Console.WriteLine("  • Minimum and Maximum");
    Console.WriteLine("  • Positive / Negative / Zero");
    Console.WriteLine("  • Even / Odd whole numbers");
    Console.WriteLine("  • Unique values");
    Console.WriteLine("  • Duplicate occurrences and duplicate values");
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("  TEXT INPUT");
    Console.ResetColor();

    Console.WriteLine("  Example:");
    Console.WriteLine("    I am learning CSharp programming");
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("  TEXT SUMMARY");
    Console.ResetColor();

    Console.WriteLine("  • Word count");
    Console.WriteLine("  • Character count");
    Console.WriteLine("  • Characters without spaces");
    Console.WriteLine("  • Unique words (punctuation-insensitive)");
    Console.WriteLine("  • Average word length");
    Console.WriteLine("  • Longest word");
    Console.WriteLine("  • Shortest word");
    Console.WriteLine("  • Word frequency table");
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("  COMMANDS");
    Console.ResetColor();

    Console.WriteLine("  • back  - Return to main menu");
    Console.WriteLine("  • exit  - Close the application");

    Console.ResetColor();

    Pause();
}


// ==================================================
// SECTION HEADER
// ==================================================

static void ShowSectionHeader(string title)
{
    Console.ForegroundColor = ConsoleColor.Cyan;

    Console.WriteLine("  ╔══════════════════════════════════════════╗");
    Console.WriteLine($"  ║  {title,-40}║");
    Console.WriteLine("  ╚══════════════════════════════════════════╝");

    Console.ResetColor();
    Console.WriteLine();
}


// ==================================================
// SUCCESS MESSAGE
// ==================================================

static void ShowSuccess(string message)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  [✓] {message}");
    Console.ResetColor();
}


// ==================================================
// ERROR MESSAGE
// ==================================================

static void ShowError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  [!] {message}");
    Console.ResetColor();
}


// ==================================================
// EXIT APPLICATION
// ==================================================

static void ExitApplication()
{
    Console.Clear();

    Console.ForegroundColor = ConsoleColor.Cyan;

    Console.WriteLine();
    Console.WriteLine("  ╔══════════════════════════════════════════╗");
    Console.WriteLine("  ║                                          ║");
    Console.WriteLine("  ║       Thank you for using the app!       ║");
    Console.WriteLine("  ║                                          ║");
    Console.WriteLine("  ║              Application closed.        ║");
    Console.WriteLine("  ║                                          ║");
    Console.WriteLine("  ╚══════════════════════════════════════════╝");

    Console.ResetColor();
    Console.WriteLine();
}


// ==================================================
// PAUSE
// ==================================================

static void Pause()
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Press Enter to continue...");
    Console.ResetColor();

    Console.ReadLine();
}
