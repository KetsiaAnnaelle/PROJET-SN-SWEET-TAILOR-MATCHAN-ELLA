using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using SweetTailor.Api;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://0.0.0.0:5080");
builder.Services.AddSingleton<ApiDatabase>();
builder.Services.AddSingleton<ApiSessions>();
builder.Services.AddSingleton<OrderImageStore>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api") ||
        context.Request.Path.StartsWithSegments("/api/auth") ||
        context.Request.Path.StartsWithSegments("/api/health"))
    {
        await next();
        return;
    }

    var sessions = context.RequestServices.GetRequiredService<ApiSessions>();
    var database = context.RequestServices.GetRequiredService<ApiDatabase>();
    var authorization = context.Request.Headers.Authorization.ToString();
    var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        ? authorization["Bearer ".Length..].Trim()
        : string.Empty;
    var userId = await sessions.GetUserIdAsync(token, database, context.RequestAborted);
    if (userId == null)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { message = "Session expirée. Veuillez vous reconnecter." });
        return;
    }

    context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())],
        "Bearer"));
    await next();
});

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "SweetTailor.Api" }));

app.MapPost("/api/auth/register", async (RegisterRequest request, ApiDatabase database, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length is < 2 or > 120 ||
        !ApiValidation.IsValidPhone(request.Phone) || !ApiValidation.IsValidEmail(request.Email) ||
        string.IsNullOrEmpty(request.Password) || request.Password.Length < 8 ||
        !request.Password.Any(char.IsDigit) || !request.AcceptedTerms)
    {
        return Results.BadRequest(new { message = "Vérifiez le nom, le téléphone, l'adresse e-mail, le mot de passe (8 caractères minimum dont un chiffre) et l'acceptation des conditions." });
    }

    var created = await database.RegisterAsync(request.FullName, request.Phone, request.Email, request.Password, cancellationToken);
    return created
        ? Results.Created("/api/auth/login", new { message = "Compte créé." })
        : Results.Conflict(new { message = "Cette adresse e-mail est déjà utilisée." });
}).RequireRateLimiting("auth");

app.MapPost("/api/auth/login", async (LoginRequest request, ApiDatabase database, ApiSessions sessions, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.Unauthorized();
    }

    var userId = await database.AuthenticateAsync(request.Identifier, request.Password, cancellationToken);
    if (userId == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { token = await sessions.CreateTokenAsync(userId.Value, database, cancellationToken) });
}).RequireRateLimiting("auth");

app.MapPost("/api/auth/logout", async (HttpContext context, ApiSessions sessions, ApiDatabase database) =>
{
    var authorization = context.Request.Headers.Authorization.ToString();
    var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        ? authorization["Bearer ".Length..].Trim()
        : string.Empty;
    if (!string.IsNullOrWhiteSpace(token))
    {
        await sessions.RevokeTokenAsync(token, database, context.RequestAborted);
    }
    return Results.NoContent();
});

app.MapPost("/api/auth/password-reset", () =>
    Results.Problem("Configurez un fournisseur e-mail/SMS avant d'activer la réinitialisation du mot de passe.",
        statusCode: StatusCodes.Status503ServiceUnavailable))
    .RequireRateLimiting("auth");

app.MapGet("/api/profile", async (HttpContext context, ApiDatabase database, CancellationToken cancellationToken) =>
{
    var profile = await database.GetWorkshopProfileAsync(context.User.GetApiUserId(), cancellationToken);
    return profile == null ? Results.NotFound() : Results.Ok(profile);
});

app.MapPut("/api/profile", async (HttpContext context, WorkshopProfile profile, ApiDatabase database, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(profile.FullName) || string.IsNullOrWhiteSpace(profile.Phone) ||
        !ApiValidation.IsValidEmail(profile.Email) || !ApiValidation.IsValidPhone(profile.Phone) ||
        string.IsNullOrWhiteSpace(profile.WorkshopName))
    {
        return Results.BadRequest(new { message = "Nom, téléphone, e-mail et nom d’atelier sont obligatoires." });
    }

    try
    {
        await database.UpdateWorkshopProfileAsync(context.User.GetApiUserId(), profile, cancellationToken);
        return Results.Ok(await database.GetWorkshopProfileAsync(context.User.GetApiUserId(), cancellationToken));
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
    {
        return Results.Conflict(new { message = "Cette adresse e-mail est déjà utilisée par un autre compte." });
    }
});

app.MapGet("/api/profile/stats", async (HttpContext context, ApiDatabase database, CancellationToken cancellationToken) =>
    Results.Ok(await database.GetWorkshopStatsAsync(context.User.GetApiUserId(), cancellationToken)));

app.MapGet("/api/clients", async (HttpContext context, string? q, ApiDatabase database, CancellationToken cancellationToken) =>
    Results.Ok(await database.GetClientsAsync(q, context.User.GetApiUserId(), cancellationToken)));

app.MapGet("/api/clients/count", async (HttpContext context, ApiDatabase database, CancellationToken cancellationToken) =>
    Results.Ok(await database.GetClientCountAsync(context.User.GetApiUserId(), cancellationToken)));

app.MapGet("/api/clients/deliveries/count", async (HttpContext context, ApiDatabase database, CancellationToken cancellationToken) =>
    Results.Ok(await database.GetActiveDeliveriesCountAsync(context.User.GetApiUserId(), cancellationToken)));

app.MapGet("/api/clients/{id:int}", async (HttpContext context, int id, ApiDatabase database, CancellationToken cancellationToken) =>
{
    var client = await database.GetClientAsync(id, context.User.GetApiUserId(), cancellationToken);
    return client == null ? Results.NotFound() : Results.Ok(client);
});

app.MapPost("/api/clients", async (HttpContext context, Client client, ApiDatabase database, CancellationToken cancellationToken) =>
{
    if (!ApiValidation.IsValidClient(client))
    {
        return Results.BadRequest(new { message = ApiValidation.ClientError });
    }
    client.Id = 0;
    client.CreatedAt = DateTime.Now;
    try
    {
        var saved = await database.SaveClientAsync(client, context.User.GetApiUserId(), cancellationToken);
        return Results.Created($"/api/clients/{saved.Id}", saved);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/clients/{id:int}", async (HttpContext context, int id, Client client, ApiDatabase database, CancellationToken cancellationToken) =>
{
    if (!ApiValidation.IsValidClient(client))
    {
        return Results.BadRequest(new { message = ApiValidation.ClientError });
    }
    client.Id = id;
    try
    {
        return Results.Ok(await database.SaveClientAsync(client, context.User.GetApiUserId(), cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapDelete("/api/clients/{id:int}", async (HttpContext context, int id, ApiDatabase database,
    OrderImageStore imageStore, CancellationToken cancellationToken) =>
{
    var ownerUserId = context.User.GetApiUserId();
    var orders = await database.GetOrdersAsync(null, id, null, ownerUserId, cancellationToken);
    if (!await database.DeleteClientAsync(id, ownerUserId, cancellationToken))
    {
        return Results.NotFound();
    }

    foreach (var order in orders)
    {
        imageStore.Delete(order.ImagePath);
    }
    return Results.NoContent();
});

app.MapPost("/api/clients/{id:int}/notes", async (HttpContext context, int id, NoteRequest request, ApiDatabase database, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest(new { message = "Le contenu de la note est obligatoire." });
    }
    try
    {
        return Results.Ok(await database.AddNoteAsync(id, request.Content, request.IsImportant, context.User.GetApiUserId(), cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

app.MapDelete("/api/clients/{clientId:int}/notes/{noteId:int}", async (HttpContext context, int clientId, int noteId, ApiDatabase database, CancellationToken cancellationToken) =>
    await database.DeleteNoteAsync(clientId, noteId, context.User.GetApiUserId(), cancellationToken) ? Results.NoContent() : Results.NotFound());

app.MapPut("/api/clients/{id:int}/measurements", async (HttpContext context, int id, List<MeasurementItem> measurements, ApiDatabase database, CancellationToken cancellationToken) =>
{
    if (!ApiValidation.IsValidMeasurements(measurements))
    {
        return Results.BadRequest(new { message = "Les mensurations doivent avoir un nom, une valeur comprise entre 0 et 500 cm et une catégorie valide (50 mesures maximum)." });
    }
    return await database.UpdateMeasurementsAsync(id, measurements, context.User.GetApiUserId(), cancellationToken)
        ? Results.NoContent()
        : Results.NotFound();
});

app.MapGet("/api/orders", async (HttpContext context, string? status, int? clientId, string? q, ApiDatabase database, CancellationToken cancellationToken) =>
{
    OrderStatus? filter = null;
    if (!string.IsNullOrWhiteSpace(status))
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var parsed))
        {
            return Results.BadRequest(new { message = "Statut de commande invalide." });
        }
        filter = parsed;
    }
    return Results.Ok(await database.GetOrdersAsync(filter?.ToString(), clientId, q, context.User.GetApiUserId(), cancellationToken));
});

app.MapGet("/api/orders/{id:int}", async (HttpContext context, int id, ApiDatabase database, CancellationToken cancellationToken) =>
{
    var order = await database.GetOrderAsync(id, context.User.GetApiUserId(), cancellationToken);
    return order == null ? Results.NotFound() : Results.Ok(order);
});

app.MapPost("/api/orders", async (HttpContext context, Order order, ApiDatabase database, CancellationToken cancellationToken) =>
{
    try
    {
        order.Id = 0;
        var saved = await database.SaveOrderAsync(order, context.User.GetApiUserId(), cancellationToken);
        return Results.Created($"/api/orders/{saved.Id}", saved);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/orders/{id:int}", async (HttpContext context, int id, Order order, ApiDatabase database, CancellationToken cancellationToken) =>
{
    order.Id = id;
    try
    {
        return Results.Ok(await database.SaveOrderAsync(order, context.User.GetApiUserId(), cancellationToken));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (OrderPaymentRequiredException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
});

app.MapDelete("/api/orders/{id:int}", async (HttpContext context, int id, ApiDatabase database,
    OrderImageStore imageStore, CancellationToken cancellationToken) =>
{
    var ownerUserId = context.User.GetApiUserId();
    var order = await database.GetOrderAsync(id, ownerUserId, cancellationToken);
    if (order == null || !await database.DeleteOrderAsync(id, ownerUserId, cancellationToken))
    {
        return Results.NotFound();
    }

    imageStore.Delete(order.ImagePath);
    return Results.NoContent();
});

app.MapPost("/api/orders/{id:int}/image", async Task<IResult> (
    HttpContext context, int id, ApiDatabase database, OrderImageStore imageStore,
    CancellationToken cancellationToken) =>
{
    if (!context.Request.HasFormContentType)
    {
        return Results.BadRequest(new { message = "Envoyez la photo au format multipart/form-data." });
    }

    if (context.Request.ContentLength > OrderImageStore.MaxImageBytes + 64 * 1024)
    {
        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
    }

    var bodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (bodySizeFeature is { IsReadOnly: false })
    {
        bodySizeFeature.MaxRequestBodySize = OrderImageStore.MaxImageBytes + 64 * 1024;
    }

    var ownerUserId = context.User.GetApiUserId();
    var order = await database.GetOrderAsync(id, ownerUserId, cancellationToken);
    if (order == null)
    {
        return Results.NotFound();
    }

    var form = await context.Request.ReadFormAsync(cancellationToken);
    var image = form.Files.GetFile("image");
    if (image == null || image.Length == 0)
    {
        return Results.BadRequest(new { message = "Sélectionnez une photo du modèle." });
    }
    if (image.Length > OrderImageStore.MaxImageBytes)
    {
        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
    }

    string fileName;
    try
    {
        await using var stream = image.OpenReadStream();
        fileName = await imageStore.SaveAsync(stream, cancellationToken);
    }
    catch (InvalidDataException exception)
    {
        return Results.BadRequest(new { message = exception.Message });
    }

    try
    {
        if (!await database.UpdateOrderImagePathAsync(id, fileName, ownerUserId, cancellationToken))
        {
            imageStore.Delete(fileName);
            return Results.NotFound();
        }
    }
    catch
    {
        imageStore.Delete(fileName);
        throw;
    }

    imageStore.Delete(order.ImagePath);
    return Results.Ok(new ImageUploadResponse(fileName));
});

app.MapGet("/api/orders/{id:int}/image", async Task<IResult> (
    HttpContext context, int id, ApiDatabase database, OrderImageStore imageStore,
    CancellationToken cancellationToken) =>
{
    var order = await database.GetOrderAsync(id, context.User.GetApiUserId(), cancellationToken);
    if (order == null)
    {
        return Results.NotFound();
    }

    var image = imageStore.OpenRead(order.ImagePath);
    return image == null
        ? Results.NotFound()
        : Results.File(image, OrderImageStore.GetContentType(order.ImagePath), enableRangeProcessing: true);
});

app.MapPost("/api/orders/{id:int}/advance", async (HttpContext context, int id, ApiDatabase database, CancellationToken cancellationToken) =>
{
    var ownerUserId = context.User.GetApiUserId();
    var order = await database.GetOrderAsync(id, ownerUserId, cancellationToken);
    if (order == null)
    {
        return Results.NotFound();
    }
    order.Status = order.Status switch
    {
        OrderStatus.Recue => OrderStatus.Couture,
        OrderStatus.Couture => OrderStatus.Essayage,
        OrderStatus.Essayage => OrderStatus.Prete,
        OrderStatus.Prete => OrderStatus.Livree,
        _ => OrderStatus.Livree
    };
    var activeStep = (int)order.Status;
    for (var index = 0; index < order.Steps.Count; index++)
    {
        order.Steps[index].State = index < activeStep
            ? StepState.Done
            : index == activeStep && order.Status != OrderStatus.Livree
                ? StepState.Now
                : order.Status == OrderStatus.Livree && index == order.Steps.Count - 1
                    ? StepState.Done
                    : StepState.Todo;
    }
    try
    {
        return Results.Ok(await database.SaveOrderAsync(order, ownerUserId, cancellationToken));
    }
    catch (OrderPaymentRequiredException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
});

app.MapGet("/api/orders/count/active", async (HttpContext context, ApiDatabase database, CancellationToken cancellationToken) =>
    Results.Ok(await database.GetActiveOrdersCountAsync(context.User.GetApiUserId(), cancellationToken)));

app.MapGet("/api/search", async (HttpContext context, string? q, string? category, ApiDatabase database, CancellationToken cancellationToken) =>
{
    var ownerUserId = context.User.GetApiUserId();
    var query = q?.Trim() ?? string.Empty;
    var scope = string.IsNullOrWhiteSpace(category) ? "Tout" : category;
    var clients = scope is "Tout" or "Clients"
        ? await database.GetClientsAsync(query, ownerUserId, cancellationToken)
        : [];
    var orders = scope is "Tout" or "Commandes"
        ? await database.GetOrdersAsync(null, null, query, ownerUserId, cancellationToken)
        : [];
    return Results.Ok(new { clients, orders });
});

app.MapGet("/api/catalog", async (HttpContext context, string? category, string? audience, string? q, ApiDatabase database, CancellationToken cancellationToken) =>
    Results.Ok(await database.GetCatalogAsync(category, q, context.User.GetApiUserId(), cancellationToken, audience)));

app.MapPost("/api/catalog", async Task<IResult> (
    HttpContext context, ApiDatabase database, OrderImageStore imageStore, CancellationToken cancellationToken) =>
{
    if (!context.Request.HasFormContentType)
    {
        return Results.BadRequest(new { message = "Envoyez le modèle et sa photo au format multipart/form-data." });
    }
    if (context.Request.ContentLength > OrderImageStore.MaxImageBytes + 128 * 1024)
    {
        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
    }
    var bodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (bodySizeFeature is { IsReadOnly: false })
    {
        bodySizeFeature.MaxRequestBodySize = OrderImageStore.MaxImageBytes + 128 * 1024;
    }

    var form = await context.Request.ReadFormAsync(cancellationToken);
    CatalogModel? model;
    try
    {
        model = JsonSerializer.Deserialize<CatalogModel>(form["model"].ToString());
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { message = "Les informations du modèle sont invalides." });
    }
    if (model == null)
    {
        return Results.BadRequest(new { message = "Les informations du modèle sont obligatoires." });
    }
    var image = form.Files.GetFile("image");
    if (image == null || image.Length == 0)
    {
        return Results.BadRequest(new { message = "Sélectionnez une image du modèle." });
    }
    if (image.Length > OrderImageStore.MaxImageBytes)
    {
        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
    }

    string? storedImageName = null;
    var savedModel = false;
    try
    {
        await using var stream = image.OpenReadStream();
        storedImageName = await imageStore.SaveAsync(stream, cancellationToken);
        model.ImagePath = storedImageName;
        model.Id = 0;
        var saved = await database.CreateCatalogModelAsync(model, context.User.GetApiUserId(), cancellationToken);
        savedModel = true;
        return Results.Created($"/api/catalog/{saved.Id}", saved);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (InvalidDataException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    finally
    {
        if (!savedModel && storedImageName != null)
        {
            imageStore.Delete(storedImageName);
        }
    }
});

app.MapGet("/api/catalog/{id:int}/image", async (HttpContext context, int id, ApiDatabase database,
    OrderImageStore imageStore, CancellationToken cancellationToken) =>
{
    var model = await database.GetCatalogModelAsync(id, context.User.GetApiUserId(), cancellationToken);
    var image = model == null ? null : imageStore.OpenRead(model.ImagePath);
    return image == null
        ? Results.NotFound()
        : Results.File(image, OrderImageStore.GetContentType(model!.ImagePath), enableRangeProcessing: true);
});

app.MapGet("/api/catalog/{id:int}", async (HttpContext context, int id, ApiDatabase database, CancellationToken cancellationToken) =>
{
    var model = await database.GetCatalogModelAsync(id, context.User.GetApiUserId(), cancellationToken);
    return model == null ? Results.NotFound() : Results.Ok(model);
});

app.MapPost("/api/catalog/{id:int}/favorite", async (HttpContext context, int id, ApiDatabase database, CancellationToken cancellationToken) =>
{
    var ownerUserId = context.User.GetApiUserId();
    if (await database.GetCatalogModelAsync(id, ownerUserId, cancellationToken) == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(await database.ToggleCatalogFavoriteAsync(id, ownerUserId, cancellationToken));
});

app.MapGet("/api/catalog/{id:int}/similar", async (HttpContext context, int id, ApiDatabase database, CancellationToken cancellationToken) =>
{
    var ownerUserId = context.User.GetApiUserId();
    var model = await database.GetCatalogModelAsync(id, ownerUserId, cancellationToken);
    if (model == null)
    {
        return Results.NotFound();
    }
    var similar = await database.GetCatalogAsync(null, null, ownerUserId, cancellationToken);
    return Results.Ok(similar
        .Where(candidate => candidate.Id != id &&
            (model.SimilarModelIds.Contains(candidate.Id) || model.Category == candidate.Category))
        .Take(3));
});

app.Run();

public partial class Program { }

public sealed record RegisterRequest(string FullName, string Phone, string Email, string Password, bool AcceptedTerms = false);
public sealed record LoginRequest(string Identifier, string Password);
public sealed record NoteRequest(string Content, bool IsImportant = false);
public sealed record ImageUploadResponse(string ImagePath);

public sealed class ApiSessions
{
    public async Task<string> CreateTokenAsync(int userId, ApiDatabase database, CancellationToken cancellationToken)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiresAt = DateTimeOffset.UtcNow.AddHours(12);
        await database.SaveSessionAsync(userId, token, expiresAt, cancellationToken);
        return token;
    }

    public Task<int?> GetUserIdAsync(string token, ApiDatabase database, CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(token)
            ? Task.FromResult<int?>(null)
            : database.GetSessionUserIdAsync(token, cancellationToken);

    public async Task RevokeTokenAsync(string token, ApiDatabase database, CancellationToken cancellationToken)
    {
        await database.DeleteSessionAsync(token, cancellationToken);
    }
}

public static class ApiPrincipalExtensions
{
    public static int GetApiUserId(this ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : throw new InvalidOperationException("L'identité de la session est absente.");
}

public static class ApiValidation
{
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhonePattern = new(@"^[+\d\s().-]+$", RegexOptions.Compiled);

    public const string ClientError = "Le nom doit contenir entre 2 et 120 caractères et le téléphone entre 9 et 15 chiffres.";

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailPattern.IsMatch(email.Trim());

    public static bool IsValidPhone(string? phone) =>
        !string.IsNullOrWhiteSpace(phone) &&
        phone.Trim().Length <= 30 &&
        PhonePattern.IsMatch(phone.Trim()) &&
        phone.Count(char.IsDigit) is >= 9 and <= 15;

    public static bool IsValidClient(Client client) =>
        !string.IsNullOrWhiteSpace(client.FullName) &&
        client.FullName.Trim().Length is >= 2 and <= 120 &&
        IsValidPhone(client.PhoneNumber) &&
        client.Address?.Length <= 300 &&
        Enum.IsDefined(client.Category);

    public static bool IsValidMeasurements(IReadOnlyCollection<MeasurementItem> measurements) =>
        measurements.Count <= 50 &&
        measurements.All(item =>
            !string.IsNullOrWhiteSpace(item.Name) &&
            item.Name.Length <= 100 &&
            double.IsFinite(item.Value) &&
            item.Value is >= 0 and <= 500 &&
            !string.IsNullOrWhiteSpace(item.Unit) &&
            item.Unit.Length <= 10 &&
            Enum.IsDefined(item.Category));
}
