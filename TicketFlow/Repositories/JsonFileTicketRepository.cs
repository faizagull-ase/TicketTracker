using System.Text.Json;
using System.Text.Json.Serialization;
using TicketFlow.Models;

namespace TicketFlow.Repositories;

/// <summary>
/// Persists tickets to a JSON file on disk, so data survives an app restart.
/// The whole file is read once (lazily, on first use) into an in-memory
/// cache, and rewritten after every add/update. Saves go through a temp file
/// + atomic rename so a crash mid-write can't leave tickets.json
/// half-written.
/// </summary>
public class JsonFileTicketRepository : ITicketRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;
    private readonly Dictionary<Guid, Ticket> _tickets = new();
    private bool _loaded;

    public JsonFileTicketRepository(string filePath)
    {
        _filePath = filePath;
    }
    
    public Task InitializeAsync() => EnsureLoadedAsync();

    public async Task<Ticket> AddAsync(Ticket ticket)
    {
        await EnsureLoadedAsync();

        _tickets[ticket.Id] = ticket;
        await SaveAsync(rollback: () => _tickets.Remove(ticket.Id));
        return ticket;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        await EnsureLoadedAsync();
        return _tickets.TryGetValue(id, out Ticket? ticket) ? ticket : null;
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync()
    {
        await EnsureLoadedAsync();
        return _tickets.Values.ToList();
    }

    public async Task<Ticket> UpdateAsync(Ticket ticket)
    {
        await EnsureLoadedAsync();

        if (!_tickets.TryGetValue(ticket.Id, out Ticket? previous))
        {
            throw new InvalidOperationException($"Cannot update ticket '{ticket.Id}' because it does not exist.");
        }

        _tickets[ticket.Id] = ticket;
        await SaveAsync(rollback: () => _tickets[ticket.Id] = previous);
        return ticket;
    }

    private async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        if (File.Exists(_filePath))
        {
            await LoadFromDiskAsync();
        }

        _loaded = true;
    }

    private async Task LoadFromDiskAsync()
    {
        List<Ticket?>? tickets;

        try
        {
            await using FileStream stream = File.OpenRead(_filePath);
            tickets = await JsonSerializer.DeserializeAsync<List<Ticket?>>(stream, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new TicketStoreCorruptException(_filePath, $"'{_filePath}' is not valid JSON ({ex.Message}).", ex);
        }
        catch (IOException ex)
        {
            throw new TicketStoreIOException(_filePath, $"Could not read '{_filePath}': {ex.Message}", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new TicketStoreIOException(_filePath, $"Could not read '{_filePath}': {ex.Message}", ex);
        }

        if (tickets is null)
        {
            return;
        }

        for (int i = 0; i < tickets.Count; i++)
        {
            Ticket? ticket = tickets[i];

            if (ticket is null || ticket.Id == Guid.Empty || string.IsNullOrWhiteSpace(ticket.Title))
            {
                throw new TicketStoreCorruptException(
                    _filePath, $"'{_filePath}' entry #{i + 1} is missing a required field (id/title).");
            }

            _tickets[ticket.Id] = ticket;
        }
    }

    private async Task SaveAsync(Action rollback)
    {
        string tempPath = _filePath + ".tmp";

        try
        {
            List<Ticket> snapshot = _tickets.Values.OrderBy(t => t.CreatedAt).ToList();

            await using (FileStream stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, snapshot, JsonOptions);
            }

            File.Move(tempPath, _filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            rollback();

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw new TicketStoreIOException(
                _filePath, $"Could not save '{_filePath}': {ex.Message}. Your change was not persisted.", ex);
        }
    }
}
