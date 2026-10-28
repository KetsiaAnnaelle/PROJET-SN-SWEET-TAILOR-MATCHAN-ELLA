using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;

namespace SweetTailor.App.Services;

public sealed class ApiSession
{
    private const string TokenKey = "sweet-tailor-api-token";
    private string? _token;
    private bool _tokenLoaded;

    public async Task SetTokenAsync(string token)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
        _token = token;
        _tokenLoaded = true;
    }

    public void Clear()
    {
        SecureStorage.Default.Remove(TokenKey);
        _token = null;
        _tokenLoaded = true;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        if (!_tokenLoaded)
        {
            _token = await SecureStorage.Default.GetAsync(TokenKey);
            _tokenLoaded = true;
        }

        if (!string.IsNullOrWhiteSpace(_token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Clear();
        }
        return response;
    }
}

internal static class ApiHttp
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly record struct ApiError(string? Message, string? Detail, string? Title);

    public static async Task<T> GetAsync<T>(HttpClient client, ApiSession session, string path, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        using var response = await session.SendAsync(client, request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidDataException($"L’API a renvoyé une réponse vide pour {path}.");
    }

    public static async Task<T> SendAsync<T>(HttpClient client, ApiSession session, HttpMethod method, string path,
        object? body = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body != null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await session.SendAsync(client, request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidDataException($"L’API a renvoyé une réponse vide pour {path}.");
    }

    public static async Task SendAsync(HttpClient client, ApiSession session, HttpMethod method, string path,
        object? body = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body != null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await session.SendAsync(client, request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public static async Task<string> GetErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions, cancellationToken);
            return !string.IsNullOrWhiteSpace(error.Message) ? error.Message
                : !string.IsNullOrWhiteSpace(error.Detail) ? error.Detail
                : !string.IsNullOrWhiteSpace(error.Title) ? error.Title
                : $"Erreur HTTP {(int)response.StatusCode}.";
        }
        catch (JsonException)
        {
            return $"Erreur HTTP {(int)response.StatusCode}.";
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await GetErrorMessageAsync(response, cancellationToken), null, response.StatusCode);
        }
    }
}

public sealed class ApiClientService(HttpClient httpClient, ApiSession session) : IClientService
{
    public async Task<List<Client>> GetClientsAsync() =>
        await ApiHttp.GetAsync<List<Client>>(httpClient, session, "api/clients");

    public Task<List<Client>> GetAllClientsAsync() => GetClientsAsync();

    public async Task<Client?> GetClientByIdAsync(int id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/clients/{id}");
        using var response = await session.SendAsync(httpClient, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }
        return await response.Content.ReadFromJsonAsync<Client>();
    }

    public async Task<List<Client>> SearchClientsAsync(string query, string filterChip = "Tous")
    {
        var path = string.IsNullOrWhiteSpace(query)
            ? "api/clients"
            : $"api/clients?q={Uri.EscapeDataString(query.Trim())}";
        var clients = await ApiHttp.GetAsync<List<Client>>(httpClient, session, path);
        return ClientService.NormalizeFilter(filterChip) switch
        {
            "En cours" => clients.Where(client => client.ActiveOrdersCount > 0).ToList(),
            "À livrer" => clients.Where(client => client.CurrentOrderStatus is OrderStatus.Prete or OrderStatus.Essayage).ToList(),
            "Livrés" => clients.Where(client => client.CurrentOrderStatus == OrderStatus.Livree).ToList(),
            _ => clients
        };
    }

    public Task<Client> AddClientAsync(Client client) =>
        ApiHttp.SendAsync<Client>(httpClient, session, HttpMethod.Post, "api/clients", client);

    public Task<bool> UpdateClientAsync(Client client) =>
        UpdateClientCoreAsync(client);

    private async Task<bool> UpdateClientCoreAsync(Client client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/clients/{client.Id}")
        {
            Content = JsonContent.Create(client)
        };
        using var response = await session.SendAsync(httpClient, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }
        return true;
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/clients/{id}");
        using var response = await session.SendAsync(httpClient, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }
        return true;
    }

    public async Task<bool> DeleteNoteAsync(int clientId, int noteId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/clients/{clientId}/notes/{noteId}");
        using var response = await session.SendAsync(httpClient, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }
        return true;
    }

    public Task<ClientNote> AddNoteAsync(int clientId, string content, bool isImportant = false) =>
        ApiHttp.SendAsync<ClientNote>(httpClient, session, HttpMethod.Post, $"api/clients/{clientId}/notes", new { content, isImportant });

    public async Task<bool> UpdateMeasurementsAsync(int clientId, List<MeasurementItem> measurements)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/clients/{clientId}/measurements")
        {
            Content = JsonContent.Create(measurements)
        };
        using var response = await session.SendAsync(httpClient, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }
        return true;
    }

    public Task<int> GetClientsCountAsync() => ApiHttp.GetAsync<int>(httpClient, session, "api/clients/count");

    public Task<int> GetActiveDeliveriesCountAsync() => ApiHttp.GetAsync<int>(httpClient, session, "api/clients/deliveries/count");
}

public sealed class ApiOrderService(HttpClient httpClient, ApiSession session) : IOrderService
{
    public Task<List<Order>> GetAllOrdersAsync() =>
        ApiHttp.GetAsync<List<Order>>(httpClient, session, "api/orders");

    public Task<List<Order>> SearchOrdersAsync(string query)
    {
        var path = string.IsNullOrWhiteSpace(query)
            ? "api/orders"
            : $"api/orders?q={Uri.EscapeDataString(query.Trim())}";
        return ApiHttp.GetAsync<List<Order>>(httpClient, session, path);
    }

    public Task<List<Order>> GetOrdersByFilterAsync(string statusFilter)
    {
        var status = statusFilter switch
        {
            "Reçues" => nameof(OrderStatus.Recue),
            "Couture" => nameof(OrderStatus.Couture),
            "Essayage" => nameof(OrderStatus.Essayage),
            "Prêtes" => nameof(OrderStatus.Prete),
            "Livrée" or "Livrées" => nameof(OrderStatus.Livree),
            _ => string.Empty
        };
        var path = string.IsNullOrEmpty(status) ? "api/orders" : $"api/orders?status={status}";
        return ApiHttp.GetAsync<List<Order>>(httpClient, session, path);
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/orders/{id}");
        using var response = await session.SendAsync(httpClient, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }
        var order = await response.Content.ReadFromJsonAsync<Order>();
        if (order == null)
        {
            return null;
        }

        order.RemoteImagePath = IsStoredImageName(order.ImagePath) ? order.ImagePath : null;
        order.ImagePath = order.RemoteImagePath == null
            ? string.Empty
            : await DownloadOrderImageAsync(order.Id);
        return order;
    }

    public Task<List<Order>> GetOrdersForClientAsync(int clientId) =>
        ApiHttp.GetAsync<List<Order>>(httpClient, session, $"api/orders?clientId={clientId}");

    public async Task<Order> CreateOrderAsync(Order order)
    {
        var selectedImagePath = order.ImagePath;
        var uploadPath = IsUploadableLocalImage(selectedImagePath) ? selectedImagePath : null;
        order.ImagePath = string.Empty;
        try
        {
            var created = await ApiHttp.SendAsync<Order>(httpClient, session, HttpMethod.Post, "api/orders", order);
            if (uploadPath != null)
            {
                var imageName = await UploadOrderImageAsync(created.Id, uploadPath);
                order.ImagePath = selectedImagePath;
                order.RemoteImagePath = imageName;
                created.ImagePath = selectedImagePath;
                created.RemoteImagePath = imageName;
            }
            return created;
        }
        finally
        {
            order.ImagePath = selectedImagePath;
        }
    }

    public async Task<bool> UpdateOrderAsync(Order order)
    {
        var selectedImagePath = order.ImagePath;
        var uploadPath = IsUploadableLocalImage(selectedImagePath) ? selectedImagePath : null;
        var storedImageName = order.RemoteImagePath ??
            (IsStoredImageName(selectedImagePath) ? selectedImagePath : string.Empty);
        order.ImagePath = storedImageName;
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/orders/{order.Id}")
        {
            Content = JsonContent.Create(order)
        };
        try
        {
            using var response = await session.SendAsync(httpClient, request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
            }

            if (uploadPath != null)
            {
                order.RemoteImagePath = await UploadOrderImageAsync(order.Id, uploadPath);
            }
            return true;
        }
        finally
        {
            order.ImagePath = selectedImagePath;
        }
    }

    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/orders/{orderId}");
        using var response = await session.SendAsync(httpClient, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }
        return true;
    }

    public async Task<bool> DeleteOrdersForClientAsync(int clientId)
    {
        var orders = await GetOrdersForClientAsync(clientId);
        foreach (var order in orders)
        {
            await DeleteOrderAsync(order.Id);
        }
        return orders.Count > 0;
    }

    public async Task<bool> AdvanceOrderStatusAsync(int orderId)
    {
        try
        {
            await ApiHttp.SendAsync<Order>(httpClient, session, HttpMethod.Post, $"api/orders/{orderId}/advance");
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            throw new OrderPaymentRequiredException();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public Task<int> GetActiveOrdersCountAsync() =>
        ApiHttp.GetAsync<int>(httpClient, session, "api/orders/count/active");

    private async Task<string> UploadOrderImageAsync(int orderId, string imagePath)
    {
        await using var fileStream = File.OpenRead(imagePath);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/orders/{orderId}/image");
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "image", Path.GetFileName(imagePath));
        request.Content = content;

        using var response = await session.SendAsync(httpClient, request);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
        return result?.ImagePath ?? throw new InvalidDataException("L’API n’a pas confirmé l’enregistrement de la photo.");
    }

    private async Task<string> DownloadOrderImageAsync(int orderId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/orders/{orderId}/image");
        using var response = await session.SendAsync(httpClient, request);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }

        var extension = response.Content.Headers.ContentType?.MediaType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new InvalidDataException("L’API a renvoyé un format d’image non pris en charge.")
        };
        var directory = Path.Combine(FileSystem.CacheDirectory, "order-images");
        Directory.CreateDirectory(directory);
        var destination = Path.Combine(directory, $"{orderId}{extension}");
        var temporaryPath = $"{destination}.tmp";
        try
        {
            await using (var output = File.Create(temporaryPath))
            await using (var input = await response.Content.ReadAsStreamAsync())
            {
                await input.CopyToAsync(output);
            }

            File.Move(temporaryPath, destination, overwrite: true);
            return destination;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static bool IsUploadableLocalImage(string path) =>
        !string.IsNullOrWhiteSpace(path) &&
        File.Exists(path) &&
        !Path.GetFullPath(path).StartsWith(
            Path.GetFullPath(Path.Combine(FileSystem.CacheDirectory, "order-images")) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);

    private static bool IsStoredImageName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal) ||
            fileName.Contains('/') ||
            fileName.Contains('\\'))
        {
            return false;
        }

        return Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _) &&
               Path.GetExtension(fileName).ToLowerInvariant() is ".jpg" or ".png" or ".webp";
    }

    private sealed record ImageUploadResponse(string ImagePath);
}

public sealed class ApiCatalogService(HttpClient httpClient, ApiSession session) : ICatalogService
{
    public async Task<List<CatalogModel>> GetCatalogAsync(string categoryFilter = "Tout", string searchQuery = "", string audienceFilter = "Tout")
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "Tout")
        {
            query.Add($"category={Uri.EscapeDataString(categoryFilter)}");
        }
        if (!string.IsNullOrWhiteSpace(audienceFilter) && audienceFilter != "Tout")
        {
            query.Add($"audience={Uri.EscapeDataString(audienceFilter)}");
        }
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query.Add($"q={Uri.EscapeDataString(searchQuery.Trim())}");
        }
        var path = "api/catalog" + (query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}");
        var models = await ApiHttp.GetAsync<List<CatalogModel>>(httpClient, session, path);
        foreach (var model in models)
        {
            await ResolveCatalogImageAsync(model);
        }
        return models;
    }

    public async Task<CatalogModel?> GetModelByIdAsync(int id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/catalog/{id}");
        using var response = await session.SendAsync(httpClient, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }
        var model = await response.Content.ReadFromJsonAsync<CatalogModel>();
        if (model != null)
        {
            await ResolveCatalogImageAsync(model);
        }
        return model;
    }

    public async Task<CatalogModel> AddModelAsync(CatalogModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (string.IsNullOrWhiteSpace(model.ImagePath) || !File.Exists(model.ImagePath))
        {
            throw new ArgumentException("Sélectionnez une image valide pour le modèle.", nameof(model));
        }

        await using var imageStream = File.OpenRead(model.ImagePath);
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(JsonSerializer.Serialize(model)), "model");
        form.Add(new StreamContent(imageStream), "image", Path.GetFileName(model.ImagePath));
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/catalog") { Content = form };
        using var response = await session.SendAsync(httpClient, request);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }

        var created = await response.Content.ReadFromJsonAsync<CatalogModel>()
            ?? throw new InvalidDataException("L’API n’a pas confirmé l’enregistrement du modèle.");
        created.RemoteImagePath = created.ImagePath;
        created.ImagePath = model.ImagePath;
        return created;
    }

    public Task<bool> ToggleFavoriteAsync(int modelId) =>
        ApiHttp.SendAsync<bool>(httpClient, session, HttpMethod.Post, $"api/catalog/{modelId}/favorite");

    public async Task<List<CatalogModel>> GetSimilarModelsAsync(int modelId)
    {
        var models = await ApiHttp.GetAsync<List<CatalogModel>>(httpClient, session, $"api/catalog/{modelId}/similar");
        foreach (var model in models)
        {
            await ResolveCatalogImageAsync(model);
        }
        return models;
    }

    private async Task ResolveCatalogImageAsync(CatalogModel model)
    {
        if (!IsStoredImageName(model.ImagePath))
        {
            return;
        }

        model.RemoteImagePath = model.ImagePath;
        var extension = Path.GetExtension(model.ImagePath).ToLowerInvariant();
        var directory = Path.Combine(FileSystem.CacheDirectory, "catalog-images");
        Directory.CreateDirectory(directory);
        var destination = Path.Combine(directory, $"{model.Id}{extension}");
        if (File.Exists(destination))
        {
            model.ImagePath = destination;
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/catalog/{model.Id}/image");
        using var response = await session.SendAsync(httpClient, request);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ApiHttp.GetErrorMessageAsync(response), null, response.StatusCode);
        }

        var contentExtension = response.Content.Headers.ContentType?.MediaType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new InvalidDataException("L’API a renvoyé un format d’image non pris en charge.")
        };
        if (contentExtension != extension)
        {
            throw new InvalidDataException("L’API a renvoyé une image dont le format ne correspond pas à son nom.");
        }
        var temporaryPath = $"{destination}.tmp";
        try
        {
            await using (var output = File.Create(temporaryPath))
            await using (var input = await response.Content.ReadAsStreamAsync())
            {
                await input.CopyToAsync(output);
            }
            File.Move(temporaryPath, destination, overwrite: true);
            model.ImagePath = destination;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static bool IsStoredImageName(string? fileName) =>
        !string.IsNullOrWhiteSpace(fileName) &&
        string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal) &&
        !fileName.Contains('/') &&
        !fileName.Contains('\\') &&
        Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _) &&
        Path.GetExtension(fileName).ToLowerInvariant() is ".jpg" or ".png" or ".webp";
}

public sealed class ApiAuthService(HttpClient httpClient, ApiSession session) : IAuthService
{
    public async Task<AuthResult> LoginAsync(string identifier, string password)
    {
        try
        {
            var result = await ApiHttp.SendAsync<LoginResponse>(httpClient, session, HttpMethod.Post, "api/auth/login",
                new { identifier, password });
            await session.SetTokenAsync(result.Token);
            return new AuthResult(true, Token: result.Token);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
        {
            return new AuthResult(false, "Identifiant ou mot de passe incorrect.");
        }
    }

    public async Task<AuthResult> RegisterAsync(string fullName, string phone, string email, string password,
        string confirmPassword, bool acceptedTerms)
    {
        if (!acceptedTerms || password != confirmPassword)
        {
            return new AuthResult(false, "Vérifiez le mot de passe et acceptez les conditions.");
        }
        try
        {
            await ApiHttp.SendAsync(httpClient, session, HttpMethod.Post, "api/auth/register",
                new { fullName, phone, email, password, acceptedTerms });
            return new AuthResult(true);
        }
        catch (HttpRequestException ex)
        {
            return new AuthResult(false, ex.Message);
        }
    }

    public async Task<AuthResult> SendPasswordResetCodeAsync(string identifier)
    {
        try
        {
            await ApiHttp.SendAsync(httpClient, session, HttpMethod.Post, "api/auth/password-reset", new { identifier });
            return new AuthResult(true);
        }
        catch (HttpRequestException ex)
        {
            return new AuthResult(false, ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await ApiHttp.SendAsync(httpClient, session, HttpMethod.Post, "api/auth/logout");
        }
        finally
        {
            session.Clear();
        }
    }

    private sealed record LoginResponse(string Token);
}

public sealed class ApiWorkshopProfileService(HttpClient httpClient, ApiSession session) : IWorkshopProfileService
{
    public Task<WorkshopProfile> GetProfileAsync() =>
        ApiHttp.GetAsync<WorkshopProfile>(httpClient, session, "api/profile");

    public Task<WorkshopProfile> UpdateProfileAsync(WorkshopProfile profile) =>
        ApiHttp.SendAsync<WorkshopProfile>(httpClient, session, HttpMethod.Put, "api/profile", profile);

    public Task<WorkshopStats> GetStatsAsync() =>
        ApiHttp.GetAsync<WorkshopStats>(httpClient, session, "api/profile/stats");
}
