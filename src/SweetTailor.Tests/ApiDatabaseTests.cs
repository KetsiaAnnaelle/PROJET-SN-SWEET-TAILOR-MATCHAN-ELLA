using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SweetTailor.Api;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;

namespace SweetTailor.Tests;

public class ApiDatabaseTests
{
    [Fact]
    public async Task ClientAndOrderCrud_PersistsAcrossReloads()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sweet-tailor-api-{Guid.NewGuid():N}.db");
        try
        {
            var database = CreateDatabase(path);
            var userId = await CreateUserAsync(database, "Rose");
            await Assert.ThrowsAsync<ArgumentException>(() =>
                database.SaveClientAsync(new Client { FullName = "R", PhoneNumber = "123" },
                    userId, CancellationToken.None));
            var client = await database.SaveClientAsync(new Client
            {
                FullName = "Rose Ndongo",
                PhoneNumber = "+237 677000000",
                Address = "Douala",
                NotesSummary = "Satin uniquement"
            }, userId, CancellationToken.None);

            Assert.True(client.Id > 0);
            Assert.NotEmpty(client.Measurements);
            Assert.Single(client.Notes);

            var order = await database.SaveOrderAsync(new Order
            {
                ClientId = client.Id,
                ModelName = "Robe de soirée",
                Fabric = "Satin",
                TotalPrice = 45000,
                AdvancePayment = 15000,
                DueDate = DateTime.Today.AddDays(10)
            }, userId, CancellationToken.None);

            Assert.True(order.Id > 0);
            Assert.Equal("Rose Ndongo", order.ClientName);
            Assert.Equal(4, order.Steps.Count);
            var imageName = $"{Guid.NewGuid():N}.jpg";
            Assert.True(await database.UpdateOrderImagePathAsync(order.Id, imageName, userId, CancellationToken.None));
            Assert.Equal(imageName, (await database.GetOrderAsync(order.Id, userId, CancellationToken.None))?.ImagePath);
            await Assert.ThrowsAsync<ArgumentException>(() =>
                database.SaveOrderAsync(new Order
                {
                    ClientId = client.Id,
                    ModelName = "Acompte invalide",
                    TotalPrice = 100,
                    AdvancePayment = 101,
                    DueDate = DateTime.Today.AddDays(1)
                }, userId, CancellationToken.None));

            client.Address = "Yaoundé";
            client.Measurements = [];
            client.Notes = [];
            var updated = await database.SaveClientAsync(client, userId, CancellationToken.None);
            Assert.Equal("Yaoundé", updated.Address);
            Assert.NotEmpty(updated.Measurements);
            Assert.Single(updated.Notes);

            order.Status = OrderStatus.Couture;
            await database.SaveOrderAsync(order, userId, CancellationToken.None);
            var advanced = await database.GetOrderAsync(order.Id, userId, CancellationToken.None);
            Assert.Equal(OrderStatus.Couture, advanced?.Status);

            var foundOrders = await database.GetOrdersAsync(null, null, "Ndongo", userId, CancellationToken.None);
            Assert.Contains(foundOrders, item => item.Id == order.Id);

            order.Status = OrderStatus.Essayage;
            await database.SaveOrderAsync(order, userId, CancellationToken.None);
            order.Status = OrderStatus.Prete;
            await Assert.ThrowsAsync<OrderPaymentRequiredException>(() =>
                database.SaveOrderAsync(order, userId, CancellationToken.None));
            order.Status = OrderStatus.Essayage;
            order.AdvancePayment = order.TotalPrice;
            await database.SaveOrderAsync(order, userId, CancellationToken.None);
            order.Status = OrderStatus.Prete;
            await database.SaveOrderAsync(order, userId, CancellationToken.None);
            Assert.Contains(await database.GetOrdersAsync(nameof(OrderStatus.Prete), null, null, userId, CancellationToken.None),
                item => item.Id == order.Id);

            Assert.True(await database.DeleteClientAsync(client.Id, userId, CancellationToken.None));
            Assert.Null(await database.GetClientAsync(client.Id, userId, CancellationToken.None));
            Assert.Null(await database.GetOrderAsync(order.Id, userId, CancellationToken.None));
        }
        finally
        {
            foreach (var leftover in Directory.GetFiles(Path.GetDirectoryName(path)!, Path.GetFileName(path) + "*"))
            {
                File.Delete(leftover);
            }
        }
    }

    [Fact]
    public async Task ClientAndOrderCrud_IsScopedToOwningUser()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sweet-tailor-tenant-{Guid.NewGuid():N}.db");
        try
        {
            var database = CreateDatabase(path);
            var ownerId = await CreateUserAsync(database, "Owner");
            var otherUserId = await CreateUserAsync(database, "Other");
            var client = await database.SaveClientAsync(new Client
            {
                FullName = "Client privé",
                PhoneNumber = "677000000"
            }, ownerId, CancellationToken.None);
            var order = await database.SaveOrderAsync(new Order
            {
                ClientId = client.Id,
                ModelName = "Robe",
                TotalPrice = 5000,
                DueDate = DateTime.Today.AddDays(7)
            }, ownerId, CancellationToken.None);
            var note = await database.AddNoteAsync(client.Id, "Note privée", true, ownerId, CancellationToken.None);

            Assert.Single(await database.GetClientsAsync(null, ownerId, CancellationToken.None));
            Assert.Empty(await database.GetClientsAsync(null, otherUserId, CancellationToken.None));
            Assert.Null(await database.GetClientAsync(client.Id, otherUserId, CancellationToken.None));
            Assert.False(await database.DeleteClientAsync(client.Id, otherUserId, CancellationToken.None));
            Assert.False(await database.DeleteNoteAsync(client.Id, note.Id, otherUserId, CancellationToken.None));
            Assert.False(await database.UpdateMeasurementsAsync(client.Id, [], otherUserId, CancellationToken.None));
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                database.AddNoteAsync(client.Id, "Note non autorisée", false, otherUserId, CancellationToken.None));
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                database.SaveClientAsync(new Client { Id = client.Id, FullName = "Hack", PhoneNumber = "677000000" },
                    otherUserId, CancellationToken.None));

            Assert.Single(await database.GetOrdersAsync(null, null, null, ownerId, CancellationToken.None));
            Assert.Empty(await database.GetOrdersAsync(null, null, null, otherUserId, CancellationToken.None));
            Assert.Null(await database.GetOrderAsync(order.Id, otherUserId, CancellationToken.None));
            Assert.False(await database.DeleteOrderAsync(order.Id, otherUserId, CancellationToken.None));
            Assert.False(await database.UpdateOrderImagePathAsync(
                order.Id, $"{Guid.NewGuid():N}.jpg", otherUserId, CancellationToken.None));
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                database.SaveOrderAsync(new Order
                {
                    Id = order.Id,
                    ClientId = client.Id,
                    ModelName = "Modification interdite",
                    TotalPrice = 5000,
                    DueDate = order.DueDate
                }, otherUserId, CancellationToken.None));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                database.SaveOrderAsync(new Order
                {
                    ClientId = client.Id,
                    ModelName = "Commande via client d'un autre compte",
                    TotalPrice = 5000,
                    DueDate = order.DueDate
                }, otherUserId, CancellationToken.None));

            Assert.True(await database.ToggleCatalogFavoriteAsync(1, ownerId, CancellationToken.None));
            Assert.True((await database.GetCatalogModelAsync(1, ownerId, CancellationToken.None))!.IsFavorite);
            Assert.False((await database.GetCatalogModelAsync(1, otherUserId, CancellationToken.None))!.IsFavorite);

            var catalogModel = await database.CreateCatalogModelAsync(new CatalogModel
            {
                Title = "Tenue enfant",
                Category = "Traditionnel",
                Audience = "Enfant",
                Subtitle = "Modèle de fête",
                ImagePath = $"{Guid.NewGuid():N}.png"
            }, ownerId, CancellationToken.None);
            Assert.Contains(await database.GetCatalogAsync(null, null, ownerId, CancellationToken.None, "Enfant"),
                model => model.Id == catalogModel.Id);
            Assert.DoesNotContain(await database.GetCatalogAsync(null, null, otherUserId, CancellationToken.None),
                model => model.Id == catalogModel.Id);
            Assert.Null(await database.GetCatalogModelAsync(catalogModel.Id, otherUserId, CancellationToken.None));
        }
        finally
        {
            foreach (var leftover in Directory.GetFiles(Path.GetDirectoryName(path)!, Path.GetFileName(path) + "*"))
            {
                File.Delete(leftover);
            }
        }
    }

    [Fact]
    public async Task WorkshopProfileAndStats_AreScopedAndCalculatedFromSavedData()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sweet-tailor-profile-{Guid.NewGuid():N}.db");
        try
        {
            var database = CreateDatabase(path);
            var ownerId = await CreateUserAsync(database, "ProfileOwner");
            var otherUserId = await CreateUserAsync(database, "ProfileOther");
            var original = await database.GetWorkshopProfileAsync(ownerId, CancellationToken.None);
            Assert.Equal("ProfileOwner", original?.FullName);
            Assert.Equal("ProfileOwner Atelier", original?.WorkshopName);

            var updated = new WorkshopProfile
            {
                FullName = "Profile Owner",
                Phone = "677000001",
                Email = "profile-owner@mail.com",
                WorkshopName = "Atelier Test",
                Address = "Douala"
            };
            await database.UpdateWorkshopProfileAsync(ownerId, updated, CancellationToken.None);
            Assert.Equal(updated.WorkshopName,
                (await database.GetWorkshopProfileAsync(ownerId, CancellationToken.None))?.WorkshopName);
            Assert.Equal("ProfileOther Atelier",
                (await database.GetWorkshopProfileAsync(otherUserId, CancellationToken.None))?.WorkshopName);

            var client = await database.SaveClientAsync(new Client
            {
                FullName = "Stat Client",
                PhoneNumber = "677000002"
            }, ownerId, CancellationToken.None);
            await database.SaveOrderAsync(new Order
            {
                ClientId = client.Id,
                ModelName = "Livrée à temps",
                Status = OrderStatus.Livree,
                TotalPrice = 5000,
                DueDate = DateTime.Today.AddDays(1)
            }, ownerId, CancellationToken.None);
            var lateOrder = await database.SaveOrderAsync(new Order
            {
                ClientId = client.Id,
                ModelName = "Livrée en retard",
                TotalPrice = 5000,
                DueDate = DateTime.Today.AddDays(1)
            }, ownerId, CancellationToken.None);
            lateOrder.DueDate = DateTime.Today.AddDays(-1);
            lateOrder.Status = OrderStatus.Livree;
            await database.SaveOrderAsync(lateOrder, ownerId, CancellationToken.None);

            var stats = await database.GetWorkshopStatsAsync(ownerId, CancellationToken.None);
            Assert.Equal(1, stats.ClientCount);
            Assert.Equal(2, stats.OrdersThisMonth);
            Assert.Equal(50, stats.OnTimeRate);
            var otherStats = await database.GetWorkshopStatsAsync(otherUserId, CancellationToken.None);
            Assert.Equal(0, otherStats.ClientCount);
            Assert.Equal(0, otherStats.OrdersThisMonth);
            Assert.Equal(0, otherStats.OnTimeRate);
        }
        finally
        {
            foreach (var leftover in Directory.GetFiles(Path.GetDirectoryName(path)!, Path.GetFileName(path) + "*"))
            {
                File.Delete(leftover);
            }
        }
    }

    [Fact]
    public async Task RegisterAndAuthenticate_WorksWithHashedPassword()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sweet-tailor-auth-{Guid.NewGuid():N}.db");
        try
        {
            var database = CreateDatabase(path);
            Assert.True(await database.RegisterAsync("Rose", "677000000", "rose@mail.com", "password1", CancellationToken.None));
            Assert.False(await database.RegisterAsync("Rose", "677000000", "rose@mail.com", "password1", CancellationToken.None));
            var userId = await database.AuthenticateAsync("rose@mail.com", "password1", CancellationToken.None);
            Assert.NotNull(userId);
            Assert.Null(await database.AuthenticateAsync("rose@mail.com", "wrong", CancellationToken.None));

            await using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path};Pooling=False"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT PasswordHash FROM Users WHERE Id=$id;";
                command.Parameters.AddWithValue("$id", userId!.Value);
                var storedPassword = (string)(await command.ExecuteScalarAsync())!;
                Assert.DoesNotContain("password1", storedPassword, StringComparison.Ordinal);
            }

            const string token = "session-token-for-test";
            await database.SaveSessionAsync(userId.Value, token, DateTimeOffset.UtcNow.AddMinutes(5), CancellationToken.None);
            Assert.Equal(userId, await database.GetSessionUserIdAsync(token, CancellationToken.None));
            await using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path};Pooling=False"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT TokenHash FROM Sessions WHERE UserId=$id;";
                command.Parameters.AddWithValue("$id", userId.Value);
                var storedToken = (string)(await command.ExecuteScalarAsync())!;
                Assert.NotEqual(token, storedToken);
            }
            await database.DeleteSessionAsync(token, CancellationToken.None);
            Assert.Null(await database.GetSessionUserIdAsync(token, CancellationToken.None));
        }

        finally
        {
            foreach (var leftover in Directory.GetFiles(Path.GetDirectoryName(path)!, Path.GetFileName(path) + "*"))
            {
                File.Delete(leftover);
            }
        }
    }

    [Fact]
    public async Task OrderImageStore_ValidatesImageBytesAndRejectsUnsafeNames()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"sweet-tailor-images-{Guid.NewGuid():N}");
        try
        {
            var imageStore = new OrderImageStore(new TestHostEnvironment { ContentRootPath = directory });
            byte[] png =
            [
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x01, 0x02
            ];
            await using var validStream = new MemoryStream(png);
            var imageName = await imageStore.SaveAsync(validStream, CancellationToken.None);

            Assert.True(OrderImageStore.IsStoredImageName(imageName));
            Assert.Equal("image/png", OrderImageStore.GetContentType(imageName));
            await using (var savedImage = imageStore.OpenRead(imageName))
            {
                Assert.NotNull(savedImage);
                var savedBytes = new byte[png.Length];
                Assert.Equal(png.Length, await savedImage!.ReadAsync(savedBytes));
                Assert.Equal(png, savedBytes);
            }
            Assert.Null(imageStore.OpenRead(@"..\private.png"));

            await using var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                imageStore.SaveAsync(invalidStream, CancellationToken.None));

            imageStore.Delete(imageName);
            Assert.Null(imageStore.OpenRead(imageName));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    private static ApiDatabase CreateDatabase(string path)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Path"] = path,
                ["Database:Pooling"] = "false"
            })
            .Build();
        return new ApiDatabase(configuration, new TestHostEnvironment());
    }

    private static async Task<int> CreateUserAsync(ApiDatabase database, string name)
    {
        var email = $"{name.ToLowerInvariant()}-{Guid.NewGuid():N}@mail.com";
        Assert.True(await database.RegisterAsync(name, "677000000", email, "password1", CancellationToken.None));
        return (await database.AuthenticateAsync(email, "password1", CancellationToken.None))!.Value;
    }
}

file sealed class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "SweetTailor.Api";
    public string ContentRootPath { get; set; } = Path.GetTempPath();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
