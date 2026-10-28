using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;

namespace SweetTailor.Api;

public sealed class ApiDatabase
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private bool _initialized;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiDatabase(IConfiguration configuration, IHostEnvironment environment)
    {
        var databasePath = configuration["Database:Path"];
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            databasePath = Path.Combine(environment.ContentRootPath, "App_Data", "sweet-tailor.db");
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(databasePath));
        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = configuration.GetValue("Database:Pooling", true),
            DefaultTimeout = 5
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await _initializeLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await ExecuteAsync(connection, null, "PRAGMA journal_mode=WAL;", cancellationToken);
            await ExecuteAsync(connection, null, "PRAGMA foreign_keys=ON;", cancellationToken);
            await ExecuteAsync(connection, null, """
                CREATE TABLE IF NOT EXISTS Clients (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OwnerUserId INTEGER REFERENCES Users(Id) ON DELETE CASCADE,
                    FullName TEXT NOT NULL,
                    PhoneNumber TEXT NOT NULL,
                    Category INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    Payload TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_Clients_FullName ON Clients(FullName COLLATE NOCASE);
                CREATE INDEX IF NOT EXISTS IX_Clients_PhoneNumber ON Clients(PhoneNumber);

                CREATE TABLE IF NOT EXISTS Orders (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClientId INTEGER NOT NULL REFERENCES Clients(Id) ON DELETE CASCADE,
                    Status INTEGER NOT NULL,
                    DueDate TEXT NOT NULL,
                    ModelName TEXT NOT NULL,
                    Payload TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_Orders_ClientId ON Orders(ClientId);
                CREATE INDEX IF NOT EXISTS IX_Orders_Status_DueDate ON Orders(Status, DueDate);

                CREATE TABLE IF NOT EXISTS Catalog (
                    Id INTEGER PRIMARY KEY,
                    OwnerUserId INTEGER REFERENCES Users(Id) ON DELETE CASCADE,
                    Category TEXT NOT NULL,
                    Audience TEXT NOT NULL DEFAULT 'Femme',
                    Title TEXT NOT NULL,
                    Payload TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_Catalog_Category_Title ON Catalog(Category, Title COLLATE NOCASE);

                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FullName TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    Email TEXT NOT NULL COLLATE NOCASE UNIQUE,
                    PasswordHash TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Sessions (
                    TokenHash TEXT PRIMARY KEY,
                    UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
                    ExpiresAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_Sessions_ExpiresAt ON Sessions(ExpiresAt);

                CREATE TABLE IF NOT EXISTS WorkshopProfiles (
                    UserId INTEGER PRIMARY KEY REFERENCES Users(Id) ON DELETE CASCADE,
                    WorkshopName TEXT NOT NULL DEFAULT '',
                    Address TEXT NOT NULL DEFAULT ''
                );

                CREATE TABLE IF NOT EXISTS CatalogFavorites (
                    UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
                    CatalogId INTEGER NOT NULL REFERENCES Catalog(Id) ON DELETE CASCADE,
                    PRIMARY KEY (UserId, CatalogId)
                );
                """, cancellationToken);

            if (!await HasColumnAsync(connection, "Catalog", "OwnerUserId", cancellationToken))
            {
                await ExecuteAsync(connection, null,
                    "ALTER TABLE Catalog ADD COLUMN OwnerUserId INTEGER REFERENCES Users(Id) ON DELETE CASCADE;",
                    cancellationToken);
            }
            if (!await HasColumnAsync(connection, "Catalog", "Audience", cancellationToken))
            {
                await ExecuteAsync(connection, null,
                    "ALTER TABLE Catalog ADD COLUMN Audience TEXT NOT NULL DEFAULT 'Femme';",
                    cancellationToken);
                await ExecuteAsync(connection, null,
                    "UPDATE Catalog SET Audience='Homme' WHERE Category='Homme' COLLATE NOCASE;",
                    cancellationToken);
            }

            if (!await HasColumnAsync(connection, "Clients", "OwnerUserId", cancellationToken))
            {
                await ExecuteAsync(connection, null,
                    "ALTER TABLE Clients ADD COLUMN OwnerUserId INTEGER REFERENCES Users(Id) ON DELETE CASCADE;",
                    cancellationToken);
            }
            await ExecuteAsync(connection, null,
                "CREATE INDEX IF NOT EXISTS IX_Clients_OwnerUserId ON Clients(OwnerUserId);",
                cancellationToken);

            var catalogCount = await ScalarAsync<long>(connection, null, "SELECT COUNT(*) FROM Catalog;", cancellationToken);
            if (catalogCount == 0)
            {
                foreach (var model in MockDataSeeder.GetInitialCatalog())
                {
                    await SaveCatalogModelAsync(connection, null, model, cancellationToken);
                }
            }

            _initialized = true;
        }
        finally
        {
            _initializeLock.Release();
        }
    }

    public async Task<List<Client>> GetClientsAsync(string? query, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT c.Payload, COUNT(o.Id),
                   COALESCE(SUM(CASE WHEN o.Status<>$delivered THEN 1 ELSE 0 END), 0),
                   (SELECT Status FROM Orders WHERE ClientId=c.Id ORDER BY DueDate DESC LIMIT 1),
                   (SELECT ModelName FROM Orders WHERE ClientId=c.Id ORDER BY DueDate DESC LIMIT 1)
            FROM Clients c LEFT JOIN Orders o ON o.ClientId=c.Id
            WHERE c.OwnerUserId=$ownerUserId
              AND ($query = '' OR c.FullName LIKE $pattern COLLATE NOCASE OR c.PhoneNumber LIKE $pattern OR c.Payload LIKE $pattern COLLATE NOCASE
                   OR EXISTS (SELECT 1 FROM Orders matching WHERE matching.ClientId=c.Id AND
                       (matching.ModelName LIKE $pattern COLLATE NOCASE OR matching.Payload LIKE $pattern COLLATE NOCASE)))
            GROUP BY c.Id
            ORDER BY c.CreatedAt DESC LIMIT 500;
            """;
        command.Parameters.AddWithValue("$query", query?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$ownerUserId", ownerUserId);
        command.Parameters.AddWithValue("$pattern", $"%{query?.Trim() ?? string.Empty}%");
        command.Parameters.AddWithValue("$delivered", (int)OrderStatus.Livree);
        var results = new List<Client>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var client = Deserialize<Client>(reader.GetString(0));
            client.Orders = new List<Order>();
            client.TotalOrdersCount = reader.GetInt32(1);
            client.ActiveOrdersCount = reader.GetInt32(2);
            if (!reader.IsDBNull(3))
            {
                client.CurrentOrderStatus = (OrderStatus)reader.GetInt32(3);
                client.CurrentOrderSummary = reader.GetString(4);
            }
            else
            {
                client.CurrentOrderStatus = OrderStatus.Recue;
                client.CurrentOrderSummary = string.Empty;
            }
            results.Add(client);
        }
        return results;
    }

    public async Task<Client?> GetClientAsync(int id, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var client = await ReadModelAsync<Client>(connection,
            "SELECT Payload FROM Clients WHERE Id=$id AND OwnerUserId=$ownerUserId;", cancellationToken,
            ("$id", id), null, ("$ownerUserId", ownerUserId));
        if (client != null)
        {
            client.Orders = new List<Order>();
            await PopulateClientOrderSummaryAsync(connection, client, cancellationToken);
        }
        return client;
    }

    public async Task<Client> SaveClientAsync(Client client, int ownerUserId, CancellationToken cancellationToken)
    {
        ValidateClient(client);
        var isNew = client.Id <= 0;
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        client.AvatarInitials = Client.ComputeInitials(client.FullName);
        client.Orders = new List<Order>();

        if (isNew)
        {
            if (client.CreatedAt == default)
            {
                client.CreatedAt = DateTime.Now;
            }
            await using var insert = connection.CreateCommand();
            insert.Transaction = (SqliteTransaction)transaction;
            insert.CommandText = """
                INSERT INTO Clients (OwnerUserId, FullName, PhoneNumber, Category, CreatedAt, Payload)
                VALUES ($ownerUserId, $name, $phone, $category, $created, '');
                SELECT last_insert_rowid();
                """;
            BindClient(insert, client);
            insert.Parameters.AddWithValue("$ownerUserId", ownerUserId);
            client.Id = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));
            if (client.Measurements.Count == 0)
            {
                client.Measurements = MockDataSeeder.CreateBlankMeasurements();
            }
            if (!string.IsNullOrWhiteSpace(client.NotesSummary) && client.Notes.Count == 0)
            {
                client.Notes.Add(new ClientNote
                {
                    Id = 1,
                    ClientId = client.Id,
                    Content = client.NotesSummary.Trim(),
                    Date = DateTime.Now
                });
            }
        }
        else
        {
            var existing = await ReadModelAsync<Client>(connection,
                "SELECT Payload FROM Clients WHERE Id=$id AND OwnerUserId=$ownerUserId;",
                cancellationToken, ("$id", client.Id), (SqliteTransaction)transaction, ("$ownerUserId", ownerUserId));
            if (existing == null)
            {
                throw new KeyNotFoundException("Client introuvable.");
            }
            if (client.Measurements.Count == 0)
            {
                client.Measurements = existing.Measurements;
            }
            if (client.Notes.Count == 0)
            {
                client.Notes = existing.Notes;
            }
            if (string.IsNullOrWhiteSpace(client.PhotoPath))
            {
                client.PhotoPath = existing.PhotoPath;
            }
            client.CreatedAt = existing.CreatedAt;
        }

        await using (var save = connection.CreateCommand())
        {
            save.Transaction = (SqliteTransaction)transaction;
            save.CommandText = isNew
                ? "UPDATE Clients SET Payload=$payload WHERE Id=$id;"
                : """
                  UPDATE Clients SET FullName=$name, PhoneNumber=$phone, Category=$category, Payload=$payload
                  WHERE Id=$id AND OwnerUserId=$ownerUserId;
                  """;
            BindClient(save, client);
            save.Parameters.AddWithValue("$id", client.Id);
            save.Parameters.AddWithValue("$ownerUserId", ownerUserId);
            save.Parameters.AddWithValue("$payload", Serialize(client));
            await save.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return client;
    }

    public async Task<bool> DeleteClientAsync(int id, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Clients WHERE Id=$id AND OwnerUserId=$ownerUserId;";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$ownerUserId", ownerUserId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<ClientNote> AddNoteAsync(int clientId, string content, bool isImportant, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var client = await ReadModelAsync<Client>(connection,
            "SELECT Payload FROM Clients WHERE Id=$id AND OwnerUserId=$ownerUserId;",
            cancellationToken, ("$id", clientId), (SqliteTransaction)transaction, ("$ownerUserId", ownerUserId));
        if (client == null)
        {
            throw new KeyNotFoundException("Client introuvable.");
        }

        var note = new ClientNote
        {
            Id = client.Notes.Count == 0 ? 1 : client.Notes.Max(item => item.Id) + 1,
            ClientId = clientId,
            Content = content.Trim(),
            IsImportant = isImportant,
            Date = DateTime.Now
        };
        client.Notes.Insert(0, note);
        await UpdateClientPayloadAsync(connection, (SqliteTransaction)transaction, clientId, client, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return note;
    }

    public async Task<bool> DeleteNoteAsync(int clientId, int noteId, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var client = await ReadModelAsync<Client>(connection,
            "SELECT Payload FROM Clients WHERE Id=$id AND OwnerUserId=$ownerUserId;",
            cancellationToken, ("$id", clientId), (SqliteTransaction)transaction, ("$ownerUserId", ownerUserId));
        if (client == null)
        {
            return false;
        }

        var removed = client.Notes.RemoveAll(item => item.Id == noteId);
        if (removed == 0)
        {
            return false;
        }

        await UpdateClientPayloadAsync(connection, (SqliteTransaction)transaction, clientId, client, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateMeasurementsAsync(int clientId, List<MeasurementItem> measurements, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var client = await ReadModelAsync<Client>(connection,
            "SELECT Payload FROM Clients WHERE Id=$id AND OwnerUserId=$ownerUserId;",
            cancellationToken, ("$id", clientId), (SqliteTransaction)transaction, ("$ownerUserId", ownerUserId));
        if (client == null)
        {
            return false;
        }

        client.Measurements = measurements;
        await UpdateClientPayloadAsync(connection, (SqliteTransaction)transaction, clientId, client, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<List<Order>> GetOrdersAsync(string? status, int? clientId, string? query, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT o.Payload FROM Orders o
            JOIN Clients c ON c.Id = o.ClientId
            WHERE c.OwnerUserId=$ownerUserId
              AND ($status IS NULL OR o.Status=$status)
              AND ($clientId IS NULL OR o.ClientId=$clientId)
              AND ($query='' OR o.ModelName LIKE $pattern COLLATE NOCASE OR o.Payload LIKE $pattern COLLATE NOCASE
                   OR c.FullName LIKE $pattern COLLATE NOCASE OR c.PhoneNumber LIKE $pattern)
            ORDER BY o.DueDate LIMIT 500;
            """;
        command.Parameters.AddWithValue("$status", status == null ? DBNull.Value : (int)Enum.Parse<OrderStatus>(status, true));
        command.Parameters.AddWithValue("$ownerUserId", ownerUserId);
        command.Parameters.AddWithValue("$clientId", clientId.HasValue ? clientId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$query", query?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$pattern", $"%{query?.Trim() ?? string.Empty}%");
        var results = new List<Order>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(Deserialize<Order>(reader.GetString(0)));
        }
        return results;
    }

    public async Task<Order?> GetOrderAsync(int id, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        return await ReadModelAsync<Order>(connection,
            "SELECT o.Payload FROM Orders o JOIN Clients c ON c.Id=o.ClientId WHERE o.Id=$id AND c.OwnerUserId=$ownerUserId;",
            cancellationToken, ("$id", id), null, ("$ownerUserId", ownerUserId));
    }

    public async Task<Order> SaveOrderAsync(Order order, int ownerUserId, CancellationToken cancellationToken)
    {
        ValidateOrder(order, isNew: order.Id <= 0);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var client = await ReadModelAsync<Client>(connection,
            "SELECT Payload FROM Clients WHERE Id=$id AND OwnerUserId=$ownerUserId;",
            cancellationToken, ("$id", order.ClientId), (SqliteTransaction)transaction, ("$ownerUserId", ownerUserId));
        if (client == null)
        {
            throw new KeyNotFoundException("Le client associé à la commande est introuvable.");
        }

        Order? existingOrder = null;
        if (order.Id > 0)
        {
            existingOrder = await ReadModelAsync<Order>(connection, """
                SELECT o.Payload FROM Orders o
                JOIN Clients c ON c.Id=o.ClientId
                WHERE o.Id=$id AND c.OwnerUserId=$ownerUserId;
                """, cancellationToken, ("$id", order.Id), (SqliteTransaction)transaction,
                ("$ownerUserId", ownerUserId));
            if (existingOrder == null)
            {
                throw new KeyNotFoundException("Commande introuvable.");
            }
            if (existingOrder.Status == OrderStatus.Essayage &&
                order.Status == OrderStatus.Prete &&
                existingOrder.RemainingPayment > 0)
            {
                throw new OrderPaymentRequiredException();
            }
            order.CreatedAt = existingOrder.CreatedAt;
            order.ImagePath = existingOrder.ImagePath;
        }
        else
        {
            order.ImagePath = string.Empty;
        }

        order.DeliveredAt = order.Status != OrderStatus.Livree
            ? null
            : existingOrder?.Status == OrderStatus.Livree
                ? existingOrder.DeliveredAt ?? DateTime.Now
                : DateTime.Now;
        order.ClientName = client.FullName;
        order.ClientPhone = client.PhoneNumber;
        order.DueDateLabel = $"Pour le {order.DueDate.ToString("d MMM yyyy", new System.Globalization.CultureInfo("fr-FR"))}";
        order.IsHotDueDate = order.DueDate.Date <= DateTime.Today.AddDays(3);
        if (order.Id <= 0 && order.Steps.Count == 0)
        {
            order.Steps =
            [
                new() { Title = "Commande reçue", Subtitle = DateTime.Now.ToString("d MMM", new System.Globalization.CultureInfo("fr-FR")), State = StepState.Done },
                new() { Title = "Coupe et couture", Subtitle = "En cours", State = StepState.Now },
                new() { Title = "Essayage client", State = StepState.Todo },
                new() { Title = "Prête à retirer", State = StepState.Todo }
            ];
        }

        if (order.Id <= 0)
        {
            order.CreatedAt = DateTime.Now;
            await using var insert = connection.CreateCommand();
            insert.Transaction = (SqliteTransaction)transaction;
            insert.CommandText = """
                INSERT INTO Orders (ClientId, Status, DueDate, ModelName, Payload)
                VALUES ($clientId, $status, $dueDate, $model, '');
                SELECT last_insert_rowid();
                """;
            BindOrder(insert, order);
            order.Id = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));
            order.OrderNumber = $"#{order.Id:D4}";
            await UpdateOrderPayloadAsync(connection, (SqliteTransaction)transaction, order, cancellationToken);
        }
        else
        {
            await using var update = connection.CreateCommand();
            update.Transaction = (SqliteTransaction)transaction;
            update.CommandText = """
                UPDATE Orders SET ClientId=$clientId, Status=$status, DueDate=$dueDate, ModelName=$model, Payload=$payload
                WHERE Id=$id AND EXISTS (
                    SELECT 1 FROM Clients c WHERE c.Id=Orders.ClientId AND c.OwnerUserId=$ownerUserId
                );
                """;
            BindOrder(update, order);
            update.Parameters.AddWithValue("$id", order.Id);
            update.Parameters.AddWithValue("$ownerUserId", ownerUserId);
            update.Parameters.AddWithValue("$payload", Serialize(order));
            if (await update.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                throw new KeyNotFoundException("Commande introuvable.");
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return order;
    }

    public async Task<bool> UpdateOrderImagePathAsync(int id, string imagePath, int ownerUserId, CancellationToken cancellationToken)
    {
        if (!OrderImageStore.IsStoredImageName(imagePath))
        {
            throw new ArgumentException("Nom de fichier d’image invalide.", nameof(imagePath));
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var order = await ReadModelAsync<Order>(connection, """
            SELECT o.Payload FROM Orders o
            JOIN Clients c ON c.Id=o.ClientId
            WHERE o.Id=$id AND c.OwnerUserId=$ownerUserId;
            """, cancellationToken, ("$id", id), (SqliteTransaction)transaction,
            ("$ownerUserId", ownerUserId));
        if (order == null)
        {
            return false;
        }

        order.ImagePath = imagePath;
        await UpdateOrderPayloadAsync(connection, (SqliteTransaction)transaction, order, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteOrderAsync(int id, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM Orders WHERE Id=$id AND EXISTS (
                SELECT 1 FROM Clients c WHERE c.Id=Orders.ClientId AND c.OwnerUserId=$ownerUserId
            );
            """;
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$ownerUserId", ownerUserId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<int> GetActiveOrdersCountAsync(int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        return (int)await ScalarAsync<long>(connection, null,
            "SELECT COUNT(*) FROM Orders o JOIN Clients c ON c.Id=o.ClientId WHERE c.OwnerUserId=$ownerUserId AND o.Status<>$delivered;",
            cancellationToken, ("$delivered", (int)OrderStatus.Livree), ("$ownerUserId", ownerUserId));
    }

    public async Task<int> GetClientCountAsync(int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        return (int)await ScalarAsync<long>(connection, null,
            "SELECT COUNT(*) FROM Clients WHERE OwnerUserId=$ownerUserId;", cancellationToken,
            ("$ownerUserId", ownerUserId));
    }

    public async Task<WorkshopProfile?> GetWorkshopProfileAsync(int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT u.FullName, u.Phone, u.Email,
                   COALESCE(NULLIF(p.WorkshopName, ''), u.FullName || ' Atelier'),
                   COALESCE(p.Address, '')
            FROM Users u
            LEFT JOIN WorkshopProfiles p ON p.UserId=u.Id
            WHERE u.Id=$userId;
            """;
        command.Parameters.AddWithValue("$userId", ownerUserId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new WorkshopProfile
        {
            FullName = reader.GetString(0),
            Phone = reader.GetString(1),
            Email = reader.GetString(2),
            WorkshopName = reader.GetString(3),
            Address = reader.GetString(4)
        };
    }

    public async Task UpdateWorkshopProfileAsync(int ownerUserId, WorkshopProfile profile, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using (var updateUser = connection.CreateCommand())
        {
            updateUser.Transaction = (SqliteTransaction)transaction;
            updateUser.CommandText = """
                UPDATE Users SET FullName=$fullName, Phone=$phone, Email=$email
                WHERE Id=$userId;
                """;
            updateUser.Parameters.AddWithValue("$fullName", profile.FullName.Trim());
            updateUser.Parameters.AddWithValue("$phone", profile.Phone.Trim());
            updateUser.Parameters.AddWithValue("$email", profile.Email.Trim());
            updateUser.Parameters.AddWithValue("$userId", ownerUserId);
            if (await updateUser.ExecuteNonQueryAsync(cancellationToken) != 1)
            {
                throw new KeyNotFoundException("Compte introuvable.");
            }
        }

        await using (var updateProfile = connection.CreateCommand())
        {
            updateProfile.Transaction = (SqliteTransaction)transaction;
            updateProfile.CommandText = """
                INSERT INTO WorkshopProfiles (UserId, WorkshopName, Address)
                VALUES ($userId, $workshopName, $address)
                ON CONFLICT(UserId) DO UPDATE SET
                    WorkshopName=excluded.WorkshopName,
                    Address=excluded.Address;
                """;
            updateProfile.Parameters.AddWithValue("$userId", ownerUserId);
            updateProfile.Parameters.AddWithValue("$workshopName", profile.WorkshopName.Trim());
            updateProfile.Parameters.AddWithValue("$address", profile.Address.Trim());
            await updateProfile.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<WorkshopStats> GetWorkshopStatsAsync(int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var stats = new WorkshopStats
        {
            ClientCount = (int)await ScalarAsync<long>(connection, null,
                "SELECT COUNT(*) FROM Clients WHERE OwnerUserId=$userId;", cancellationToken,
                ("$userId", ownerUserId))
        };

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT o.Payload
            FROM Orders o
            JOIN Clients c ON c.Id=o.ClientId
            WHERE c.OwnerUserId=$userId;
            """;
        command.Parameters.AddWithValue("$userId", ownerUserId);
        var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);
        var deliveredCount = 0;
        var onTimeCount = 0;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var order = Deserialize<Order>(reader.GetString(0));
            if (order.CreatedAt >= firstDayOfMonth && order.CreatedAt < firstDayOfNextMonth)
            {
                stats.OrdersThisMonth++;
            }

            if (order.Status == OrderStatus.Livree && order.DeliveredAt.HasValue)
            {
                deliveredCount++;
                if (order.DeliveredAt.Value.Date <= order.DueDate.Date)
                {
                    onTimeCount++;
                }
            }
        }

        stats.OnTimeRate = deliveredCount == 0
            ? 0
            : (int)Math.Round(onTimeCount * 100d / deliveredCount, MidpointRounding.AwayFromZero);
        return stats;
    }

    public async Task<int> GetActiveDeliveriesCountAsync(int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        return (int)await ScalarAsync<long>(connection, null,
            "SELECT COUNT(*) FROM Orders o JOIN Clients c ON c.Id=o.ClientId WHERE c.OwnerUserId=$ownerUserId AND o.Status IN ($ready, $fitting);",
            cancellationToken, ("$ready", (int)OrderStatus.Prete), ("$fitting", (int)OrderStatus.Essayage),
            ("$ownerUserId", ownerUserId));
    }

    public async Task<List<CatalogModel>> GetCatalogAsync(string? category, string? query, int ownerUserId, CancellationToken cancellationToken,
        string? audience = null)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT c.Payload, EXISTS(
                SELECT 1 FROM CatalogFavorites f WHERE f.CatalogId=c.Id AND f.UserId=$ownerUserId
            ), c.Audience
            FROM Catalog c
            WHERE (c.OwnerUserId IS NULL OR c.OwnerUserId=$ownerUserId)
              AND ($category='' OR c.Category=$category COLLATE NOCASE)
              AND ($audience='' OR c.Audience=$audience COLLATE NOCASE)
              AND ($query='' OR c.Title LIKE $pattern COLLATE NOCASE OR c.Category LIKE $pattern COLLATE NOCASE
                   OR c.Audience LIKE $pattern COLLATE NOCASE OR c.Payload LIKE $pattern COLLATE NOCASE)
            ORDER BY Title LIMIT 500;
            """;
        command.Parameters.AddWithValue("$category", string.IsNullOrWhiteSpace(category) || category == "Tout" ? string.Empty : category);
        command.Parameters.AddWithValue("$audience", string.IsNullOrWhiteSpace(audience) || audience == "Tout" ? string.Empty : audience);
        command.Parameters.AddWithValue("$ownerUserId", ownerUserId);
        command.Parameters.AddWithValue("$query", query?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$pattern", $"%{query?.Trim() ?? string.Empty}%");
        var results = new List<CatalogModel>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var model = Deserialize<CatalogModel>(reader.GetString(0));
            model.IsFavorite = reader.GetBoolean(1);
            model.Audience = reader.GetString(2);
            results.Add(model);
        }
        return results;
    }

    public async Task<CatalogModel?> GetCatalogModelAsync(int id, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT c.Payload, EXISTS(
                SELECT 1 FROM CatalogFavorites f WHERE f.CatalogId=c.Id AND f.UserId=$ownerUserId
            ), c.Audience
            FROM Catalog c WHERE c.Id=$id AND (c.OwnerUserId IS NULL OR c.OwnerUserId=$ownerUserId);
            """;
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$ownerUserId", ownerUserId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }
        var model = Deserialize<CatalogModel>(reader.GetString(0));
        model.IsFavorite = reader.GetBoolean(1);
        model.Audience = reader.GetString(2);
        return model;
    }

    public async Task<CatalogModel> CreateCatalogModelAsync(CatalogModel model, int ownerUserId, CancellationToken cancellationToken)
    {
        ValidateCatalogModel(model);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.Transaction = (SqliteTransaction)transaction;
        command.CommandText = """
            INSERT INTO Catalog (OwnerUserId, Category, Audience, Title, Payload)
            VALUES ($ownerUserId, $category, $audience, $title, '');
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$ownerUserId", ownerUserId);
        command.Parameters.AddWithValue("$category", model.Category.Trim());
        command.Parameters.AddWithValue("$audience", model.Audience.Trim());
        command.Parameters.AddWithValue("$title", model.Title.Trim());
        model.Id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        model.Title = model.Title.Trim();
        model.Category = model.Category.Trim();
        model.Audience = model.Audience.Trim();
        model.Subtitle = model.Subtitle?.Trim() ?? string.Empty;
        await SaveCatalogPayloadAsync(connection, model, cancellationToken, (SqliteTransaction)transaction);
        await transaction.CommitAsync(cancellationToken);
        return model;
    }

    public async Task<bool> ToggleCatalogFavoriteAsync(int id, int ownerUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var model = await ReadModelAsync<CatalogModel>(connection,
            "SELECT Payload FROM Catalog WHERE Id=$id AND (OwnerUserId IS NULL OR OwnerUserId=$ownerUserId);",
            cancellationToken, ("$id", id), (SqliteTransaction)transaction, ("$ownerUserId", ownerUserId));
        if (model == null)
        {
            return false;
        }
        await using var favorite = connection.CreateCommand();
        favorite.Transaction = (SqliteTransaction)transaction;
        favorite.CommandText = """
            INSERT INTO CatalogFavorites (UserId, CatalogId)
            VALUES ($userId, $catalogId)
            ON CONFLICT(UserId, CatalogId) DO NOTHING;
            """;
        favorite.Parameters.AddWithValue("$userId", ownerUserId);
        favorite.Parameters.AddWithValue("$catalogId", id);
        var inserted = await favorite.ExecuteNonQueryAsync(cancellationToken) == 1;
        if (!inserted)
        {
            await using var remove = connection.CreateCommand();
            remove.Transaction = (SqliteTransaction)transaction;
            remove.CommandText = "DELETE FROM CatalogFavorites WHERE UserId=$userId AND CatalogId=$catalogId;";
            remove.Parameters.AddWithValue("$userId", ownerUserId);
            remove.Parameters.AddWithValue("$catalogId", id);
            await remove.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
        return inserted;
    }

    public async Task<bool> RegisterAsync(string fullName, string phone, string email, string password, CancellationToken cancellationToken)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210_000, HashAlgorithmName.SHA256, 32);
        var passwordHash = $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (FullName, Phone, Email, PasswordHash) VALUES ($name, $phone, $email, $hash);";
        command.Parameters.AddWithValue("$name", fullName.Trim());
        command.Parameters.AddWithValue("$phone", phone.Trim());
        command.Parameters.AddWithValue("$email", email.Trim());
        command.Parameters.AddWithValue("$hash", passwordHash);
        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return false;
        }
    }

    public async Task<int?> AuthenticateAsync(string identifier, string password, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, PasswordHash FROM Users
            WHERE Email=$identifier COLLATE NOCASE OR Phone=$identifier LIMIT 1;
            """;
        command.Parameters.AddWithValue("$identifier", identifier.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var userId = reader.GetInt32(0);
        var storedHash = reader.GetString(1);
        var parts = storedHash.Split(':', 2);
        if (parts.Length != 2)
        {
            throw new InvalidDataException("Le hash de mot de passe enregistré est invalide.");
        }
        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210_000, HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash) ? userId : null;
    }

    public async Task SaveSessionAsync(int userId, string token, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM Sessions WHERE ExpiresAt<=$now;
            INSERT INTO Sessions (TokenHash, UserId, ExpiresAt) VALUES ($hash, $userId, $expiresAt);
            """;
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$hash", HashToken(token));
        command.Parameters.AddWithValue("$userId", userId);
        command.Parameters.AddWithValue("$expiresAt", expiresAt.ToUniversalTime().ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int?> GetSessionUserIdAsync(string token, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT UserId FROM Sessions WHERE TokenHash=$hash AND ExpiresAt>$now LIMIT 1;
            """;
        command.Parameters.AddWithValue("$hash", HashToken(token));
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }

    public async Task DeleteSessionAsync(string token, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Sessions WHERE TokenHash=$hash;";
        command.Parameters.AddWithValue("$hash", HashToken(token));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await ExecuteAsync(connection, null, "PRAGMA foreign_keys=ON; PRAGMA busy_timeout=5000;", cancellationToken);
        return connection;
    }

    private static async Task ExecuteAsync(SqliteConnection connection, SqliteTransaction? transaction, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> HasColumnAsync(SqliteConnection connection, string tableName, string columnName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static async Task<T> ScalarAsync<T>(SqliteConnection connection, SqliteTransaction? transaction, string sql,
        CancellationToken cancellationToken, params (string Name, object Value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        }
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (T)Convert.ChangeType(result!, typeof(T));
    }

    private static async Task<T?> ReadModelAsync<T>(SqliteConnection connection, string sql, CancellationToken cancellationToken,
        (string Name, object Value) parameter, SqliteTransaction? transaction = null,
        params (string Name, object Value)[] additionalParameters) where T : class
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        foreach (var additionalParameter in additionalParameters)
        {
            command.Parameters.AddWithValue(additionalParameter.Name, additionalParameter.Value);
        }
        var payload = await command.ExecuteScalarAsync(cancellationToken) as string;
        return payload == null ? null : Deserialize<T>(payload);
    }

    private static async Task UpdateClientPayloadAsync(SqliteConnection connection, SqliteTransaction transaction, int id,
        Client client, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "UPDATE Clients SET Payload=$payload WHERE Id=$id;";
        command.Parameters.AddWithValue("$payload", Serialize(client));
        command.Parameters.AddWithValue("$id", id);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task PopulateClientOrderSummaryAsync(SqliteConnection connection, Client client,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*),
                   COALESCE(SUM(CASE WHEN Status<>$delivered THEN 1 ELSE 0 END), 0),
                   (SELECT Status FROM Orders WHERE ClientId=$clientId ORDER BY DueDate DESC LIMIT 1),
                   (SELECT ModelName FROM Orders WHERE ClientId=$clientId ORDER BY DueDate DESC LIMIT 1)
            FROM Orders WHERE ClientId=$clientId;
            """;
        command.Parameters.AddWithValue("$clientId", client.Id);
        command.Parameters.AddWithValue("$delivered", (int)OrderStatus.Livree);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return;
        }

        client.TotalOrdersCount = reader.GetInt32(0);
        client.ActiveOrdersCount = reader.GetInt32(1);
        if (!reader.IsDBNull(2))
        {
            client.CurrentOrderStatus = (OrderStatus)reader.GetInt32(2);
            client.CurrentOrderSummary = reader.GetString(3);
        }
        else
        {
            client.CurrentOrderStatus = OrderStatus.Recue;
            client.CurrentOrderSummary = string.Empty;
        }
    }

    private static async Task UpdateOrderPayloadAsync(SqliteConnection connection, SqliteTransaction transaction, Order order,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "UPDATE Orders SET Payload=$payload WHERE Id=$id;";
        command.Parameters.AddWithValue("$payload", Serialize(order));
        command.Parameters.AddWithValue("$id", order.Id);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveCatalogModelAsync(SqliteConnection connection, SqliteTransaction? transaction, CatalogModel model,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO Catalog (Id, Category, Audience, Title, Payload) VALUES ($id, $category, $audience, $title, $payload)
            ON CONFLICT(Id) DO UPDATE SET Category=excluded.Category, Audience=excluded.Audience, Title=excluded.Title, Payload=excluded.Payload;
            """;
        command.Parameters.AddWithValue("$id", model.Id);
        command.Parameters.AddWithValue("$category", model.Category);
        command.Parameters.AddWithValue("$audience", model.Audience);
        command.Parameters.AddWithValue("$title", model.Title);
        command.Parameters.AddWithValue("$payload", Serialize(model));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveCatalogPayloadAsync(SqliteConnection connection, CatalogModel model,
        CancellationToken cancellationToken, SqliteTransaction? transaction = null)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE Catalog SET Audience=$audience, Payload=$payload WHERE Id=$id;
            """;
        command.Parameters.AddWithValue("$id", model.Id);
        command.Parameters.AddWithValue("$audience", model.Audience);
        command.Parameters.AddWithValue("$payload", Serialize(model));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void BindClient(SqliteCommand command, Client client)
    {
        command.Parameters.AddWithValue("$name", client.FullName.Trim());
        command.Parameters.AddWithValue("$phone", client.PhoneNumber.Trim());
        command.Parameters.AddWithValue("$category", (int)client.Category);
        command.Parameters.AddWithValue("$created", client.CreatedAt.ToString("O"));
    }

    private static void BindOrder(SqliteCommand command, Order order)
    {
        command.Parameters.AddWithValue("$clientId", order.ClientId);
        command.Parameters.AddWithValue("$status", (int)order.Status);
        command.Parameters.AddWithValue("$dueDate", order.DueDate.ToString("O"));
        command.Parameters.AddWithValue("$model", order.ModelName.Trim());
    }

    private static void ValidateOrder(Order order, bool isNew)
    {
        if (order.ClientId <= 0 || string.IsNullOrWhiteSpace(order.ModelName) || order.ModelName.Trim().Length > 120)
        {
            throw new ArgumentException("Un client et un modèle (120 caractères maximum) sont obligatoires.");
        }
        if (order.TotalPrice <= 0 || order.AdvancePayment < 0 || order.AdvancePayment > order.TotalPrice)
        {
            throw new ArgumentException("Le prix doit être positif et l'acompte compris entre 0 et le prix total.");
        }
        if (order.DueDate == default || (isNew && order.DueDate.Date < DateTime.Today))
        {
            throw new ArgumentException("L'échéance doit être renseignée et ne peut pas être dans le passé.");
        }
        if (!Enum.IsDefined(order.Status))
        {
            throw new ArgumentException("Statut de commande invalide.");
        }
        if (order.Fabric?.Length > 120)
        {
            throw new ArgumentException("Le nom du tissu ne peut pas dépasser 120 caractères.");
        }
    }

    private static void ValidateCatalogModel(CatalogModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title) || model.Title.Trim().Length is < 2 or > 120)
        {
            throw new ArgumentException("Le nom du modèle doit contenir entre 2 et 120 caractères.");
        }
        if (string.IsNullOrWhiteSpace(model.Category) || model.Category.Trim().Length > 60 ||
            model.Subtitle?.Length > 300 ||
            string.IsNullOrWhiteSpace(model.ImagePath) ||
            model.Audience is not ("Homme" or "Femme" or "Enfant"))
        {
            throw new ArgumentException("Vérifiez la catégorie, le public (homme, femme ou enfant) et la description du modèle.");
        }
    }

    private static void ValidateClient(Client client)
    {
        var phone = client.PhoneNumber?.Trim() ?? string.Empty;
        var digitCount = phone.Count(char.IsDigit);
        if (string.IsNullOrWhiteSpace(client.FullName) || client.FullName.Trim().Length is < 2 or > 120)
        {
            throw new ArgumentException("Le nom du client doit contenir entre 2 et 120 caractères.");
        }
        if (phone.Length > 30 || !phone.All(character => char.IsDigit(character) || character is '+' or ' ' or '(' or ')' or '.' or '-') ||
            digitCount is < 9 or > 15)
        {
            throw new ArgumentException("Le numéro de téléphone doit contenir entre 9 et 15 chiffres.");
        }
        if (client.Address?.Length > 300 || !Enum.IsDefined(client.Category))
        {
            throw new ArgumentException("L'adresse ou la catégorie du client est invalide.");
        }
    }

    private static string Serialize<T>(T item) => JsonSerializer.Serialize(item, JsonOptions);

    private static T Deserialize<T>(string payload)
    {
        var item = JsonSerializer.Deserialize<T>(payload, JsonOptions)
            ?? throw new InvalidDataException("Enregistrement JSON invalide.");
        if (item is Order order && !OrderImageStore.IsStoredImageName(order.ImagePath))
        {
            order.ImagePath = string.Empty;
        }

        return item;
    }
}
