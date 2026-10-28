using System.Text.Json;
using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public interface IAppDataStore
{
    Task<List<Client>?> LoadClientsAsync();
    Task SaveClientsAsync(List<Client> clients);
    Task<List<Order>?> LoadOrdersAsync();
    Task SaveOrdersAsync(List<Order> orders);
}

public sealed class JsonAppDataStore : IAppDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _directoryPath;

    public JsonAppDataStore(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        _directoryPath = directoryPath;
    }

    public Task<List<Client>?> LoadClientsAsync() => LoadAsync<Client>("clients.json");

    public Task SaveClientsAsync(List<Client> clients) => SaveAsync("clients.json", clients);

    public Task<List<Order>?> LoadOrdersAsync() => LoadAsync<Order>("orders.json");

    public Task SaveOrdersAsync(List<Order> orders) => SaveAsync("orders.json", orders);

    private async Task<List<T>?> LoadAsync<T>(string fileName)
    {
        var path = Path.Combine(_directoryPath, fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions)
            ?? throw new InvalidDataException($"Le fichier de données « {fileName} » est vide ou invalide.");
    }

    private async Task SaveAsync<T>(string fileName, List<T> items)
    {
        Directory.CreateDirectory(_directoryPath);
        var path = Path.Combine(_directoryPath, fileName);
        var temporaryPath = $"{path}.tmp";

        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, items, JsonOptions);
        }

        File.Move(temporaryPath, path, overwrite: true);
    }
}
