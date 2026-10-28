using SweetTailor.Core.Models;
using SweetTailor.Core.Services;
using SweetTailor.Core.ViewModels;
using Xunit;

namespace SweetTailor.Tests;

public class ServicesTests
{
    [Fact]
    public async Task ClientService_ReturnsSeededClients()
    {
        var service = new ClientService();
        var clients = await service.GetAllClientsAsync();

        Assert.NotEmpty(clients);
        Assert.Contains(clients, c => c.FullName == "Nadège Mbarga");
        Assert.Contains(clients, c => c.FullName == "Christelle Tchoua");
    }

    [Fact]
    public async Task ClientService_SearchFiltersCorrectly()
    {
        var service = new ClientService();
        var results = await service.SearchClientsAsync("Mbappé");

        Assert.Single(results);
        Assert.Equal("Joël Mbappé", results[0].FullName);

        var mbaResults = await service.SearchClientsAsync("Mba");
        Assert.Contains(mbaResults, c => c.FullName == "Nadège Mbarga");
        Assert.Contains(mbaResults, c => c.FullName == "Joël Mbappé");
    }

    [Fact]
    public async Task OrderService_AdvanceStatus_TransitionsThroughAllSteps()
    {
        var service = new OrderService();
        var order = await service.GetOrderByIdAsync(142); // Initially Essayage
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Essayage, order.Status);
        order.AdvancePayment = order.TotalPrice;
        Assert.True(await service.UpdateOrderAsync(order));

        // Advance to Prete
        var advanced = await service.AdvanceOrderStatusAsync(142);
        Assert.True(advanced);
        Assert.Equal(OrderStatus.Prete, order.Status);
        Assert.Contains(await service.GetOrdersByFilterAsync("Prêtes"), item => item.Id == order.Id);

        // Advance to Livree
        await service.AdvanceOrderStatusAsync(142);
        Assert.Equal(OrderStatus.Livree, order.Status);
    }

    [Fact]
    public async Task OrderService_DoesNotMarkUnpaidFittingOrderReady()
    {
        var service = new OrderService();
        var order = await service.GetOrderByIdAsync(142);

        Assert.NotNull(order);
        Assert.True(order.RemainingPayment > 0);
        await Assert.ThrowsAsync<OrderPaymentRequiredException>(() => service.AdvanceOrderStatusAsync(order.Id));
        Assert.Equal(OrderStatus.Essayage, order.Status);
    }

    [Fact]
    public void Order_CalculatesPaymentRatioAndRemainingCorrectly()
    {
        var order = new Order
        {
            TotalPrice = 45000,
            AdvancePayment = 20000
        };

        Assert.Equal(25000, order.RemainingPayment);
        Assert.Equal(20000.0 / 45000.0, order.PaymentProgressRatio, 4);
        Assert.Equal("20 000 / 45 000 FCFA", order.FormattedPaymentRatio);
    }

    [Fact]
    public void Order_StatusChangesRaisePropertyChanged()
    {
        var order = new Order();
        var changedProperties = new List<string?>();
        order.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        order.Status = OrderStatus.Couture;

        Assert.Contains(nameof(Order.Status), changedProperties);
        Assert.Contains(nameof(Order.StatusText), changedProperties);
    }

    [Fact]
    public async Task CatalogService_FilterAndFavoritesWork()
    {
        var service = new CatalogService();
        var weddingModels = await service.GetCatalogAsync("Mariage");
        Assert.Single(weddingModels);
        Assert.Equal("Robe sirène en satin", weddingModels[0].Title);

        var isFav = await service.ToggleFavoriteAsync(weddingModels[0].Id);
        Assert.True(isFav);
    }

    [Fact]
    public async Task CatalogService_AddModel_FiltersByAudience()
    {
        var service = new CatalogService();
        var model = await service.AddModelAsync(new CatalogModel
        {
            Title = "Ensemble enfant",
            Category = "Traditionnel",
            Audience = "Enfant",
            Subtitle = "Coupe confortable",
            ImagePath = @"C:\atelier\enfant.png"
        });

        var childrenModels = await service.GetCatalogAsync(audienceFilter: "Enfant");
        Assert.Contains(childrenModels, item => item.Id == model.Id);
    }

    [Fact]
    public async Task ClientService_AllClientsHaveMeasurementsAndNotes()
    {
        var service = new ClientService();
        var clients = await service.GetAllClientsAsync();

        Assert.All(clients, client =>
        {
            Assert.NotEmpty(client.Measurements);
            Assert.NotEmpty(client.Notes);
        });
    }

    [Fact]
    public async Task ClientService_AddNoteAsync_AddsNoteCorrectly()
    {
        var service = new ClientService();
        var client = (await service.GetAllClientsAsync()).First();
        int initialNoteCount = client.Notes.Count;

        var note = await service.AddNoteAsync(client.Id, "Client préfère des ourlets invisibles", isImportant: true);

        Assert.NotNull(note);
        Assert.Equal("Client préfère des ourlets invisibles", note.Content);
        Assert.True(note.IsImportant);
        Assert.Equal(initialNoteCount + 1, client.Notes.Count);
    }

    [Fact]
    public async Task MeasurementsViewModel_SaveAsync_ShowsSuccessAfterSaving()
    {
        var service = new ClientService();
        var client = (await service.GetAllClientsAsync()).First();
        var viewModel = new MeasurementsViewModel(service);
        await viewModel.LoadMeasurementsAsync(client.Id);
        viewModel.FilteredMeasurements[0].Value = 105.5;

        await viewModel.SaveAsync();

        Assert.Equal(105.5, (await service.GetClientByIdAsync(client.Id))!.Measurements
            .First(item => item.Name == viewModel.FilteredMeasurements[0].Name).Value);
        Assert.Equal("Mesures enregistrées avec succès.", viewModel.ToastMessage);
        Assert.Equal(ToastType.Success, viewModel.ToastType);
    }

    [Fact]
    public async Task ClientService_CrudPersistsChangesAndDeletesRelatedOrders()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"sweet-tailor-tests-{Guid.NewGuid():N}");
        var store = new JsonAppDataStore(directory);
        var orders = new OrderService(new DemoModeService(), dataStore: store);
        var clients = new ClientService(new DemoModeService(), dataStore: store, orderService: orders);

        try
        {
            var client = await clients.AddClientAsync(new Client
            {
                FullName = "Amina Test",
                PhoneNumber = "+237 600000000",
                Address = "Douala"
            });
            var order = await orders.CreateOrderAsync(new Order
            {
                ClientId = client.Id,
                ClientName = client.FullName,
                ModelName = "Robe test",
                TotalPrice = 50000,
                AdvancePayment = 10000,
                DueDate = DateTime.Today.AddDays(10)
            });

            client.Address = "Yaoundé";
            Assert.True(await clients.UpdateClientAsync(client));

            var reloadedClients = new ClientService(new DemoModeService(), dataStore: store);
            Assert.Equal("Yaoundé", (await reloadedClients.GetClientByIdAsync(client.Id))?.Address);

            Assert.True(await clients.DeleteClientAsync(client.Id));
            Assert.Null(await clients.GetClientByIdAsync(client.Id));
            Assert.Null(await orders.GetOrderByIdAsync(order.Id));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task OrderService_UpdateAndDeleteWork()
    {
        var service = new OrderService();
        var order = (await service.GetAllOrdersAsync()).First();
        order.ModelName = "Modèle modifié";

        Assert.True(await service.UpdateOrderAsync(order));
        Assert.Equal("Modèle modifié", (await service.GetOrderByIdAsync(order.Id))?.ModelName);
        Assert.True(await service.DeleteOrderAsync(order.Id));
        Assert.Null(await service.GetOrderByIdAsync(order.Id));
        Assert.False(await service.DeleteOrderAsync(order.Id));
    }

    [Fact]
    public async Task OrderService_CreateOrderWithModelAndImage_Works()
    {
        var service = new OrderService();
        var newOrder = new Order
        {
            ClientId = 1,
            ClientName = "Nadège Mbarga",
            ModelName = "Robe bustier en dentelle",
            Fabric = "Dentelle perlée",
            TotalPrice = 60000,
            AdvancePayment = 30000,
            ImagePath = "C:\\atelier\\models\\robe_bustier.jpg",
            Status = OrderStatus.Recue
        };

        var created = await service.CreateOrderAsync(newOrder);

        Assert.NotNull(created);
        Assert.Equal("Robe bustier en dentelle", created.ModelName);
        Assert.Equal("C:\\atelier\\models\\robe_bustier.jpg", created.ImagePath);
        Assert.Equal(OrderStatus.Recue, created.Status);
        Assert.Equal(30000, created.RemainingPayment);
    }

    [Fact]
    public async Task NewOrderViewModel_CreateOrder_PreservesImportedPhotoPath()
    {
        var viewModel = new NewOrderViewModel(new OrderService(), new ClientService())
        {
            PhotoPath = "C:\\atelier\\models\\robe_modele.jpg",
            ModelName = "Robe modelee",
            TotalPrice = 25000
        };

        var order = await viewModel.CreateOrderAsync();

        Assert.Equal(viewModel.PhotoPath, order.ImagePath);
    }

    [Fact]
    public async Task NewOrderViewModel_RejectsInvalidAmountsAndPastDueDate()
    {
        var viewModel = new NewOrderViewModel(new OrderService(), new ClientService())
        {
            SelectedClient = new Client { Id = 1, FullName = "Client test" },
            ModelName = "Robe",
            TotalPrice = 10000,
            AdvancePayment = 12000
        };

        Assert.False(viewModel.CanSave);
        await Assert.ThrowsAsync<ArgumentException>(() => viewModel.CreateOrderAsync());

        viewModel.AdvancePayment = 5000;
        viewModel.DueDate = DateTime.Today.AddDays(-1);
        Assert.False(viewModel.CanSave);
        await Assert.ThrowsAsync<ArgumentException>(() => viewModel.CreateOrderAsync());
    }

    [Fact]
    public async Task ClientService_AddClient_CreatesMeasurementTemplateAndNote()
    {
        var service = new ClientService(seed: []);
        var created = await service.AddClientAsync(new Client
        {
            FullName = "Amina Njoya",
            PhoneNumber = "+237 600000001",
            NotesSummary = "Préfère le wax"
        });

        Assert.NotEmpty(created.Measurements);
        Assert.Contains(created.Measurements, item => item.Name == "Tour de poitrine");
        Assert.Single(created.Notes);
        Assert.Equal("Préfère le wax", created.Notes[0].Content);
    }

    [Fact]
    public async Task ClientService_UpdateWithoutMeasurements_PreservesExistingOnes()
    {
        var service = new ClientService(seed: []);
        var created = await service.AddClientAsync(new Client
        {
            FullName = "Amina Njoya",
            PhoneNumber = "+237 600000001"
        });
        created.Measurements[0].Value = 92;
        await service.UpdateMeasurementsAsync(created.Id, created.Measurements);

        Assert.True(await service.UpdateClientAsync(new Client
        {
            Id = created.Id,
            FullName = "Amina Njoya",
            PhoneNumber = "+237 600000002",
            Address = "Yaoundé"
        }));

        var reloaded = await service.GetClientByIdAsync(created.Id);
        Assert.Equal(92, reloaded?.Measurements[0].Value);
        Assert.Equal("Yaoundé", reloaded?.Address);
    }

    [Fact]
    public async Task OrderService_SearchFindsClientAndModel()
    {
        var service = new OrderService();
        var byModel = await service.SearchOrdersAsync("soirée");
        var byClient = await service.SearchOrdersAsync("Mbarga");

        Assert.NotEmpty(byModel);
        Assert.NotEmpty(byClient);
    }
}

public class ValidationTests
{
    [Fact]
    public void RegisterViewModel_DetectsEmailError()
    {
        var authService = new AuthService();
        var vm = new RegisterViewModel(authService)
        {
            FullName = "Rose Ndongo",
            Phone = "+237 6 77 00 00 00",
            Email = "rose.ndongo@mail", // Invalid domain
            Password = "password1",
            ConfirmPassword = "password1",
            AcceptTerms = true
        };

        Assert.False(vm.IsEmailValid);
        Assert.True(vm.HasEmailError);
        Assert.False(vm.CanSubmit);
    }

    [Fact]
    public void RegisterViewModel_DetectsPasswordMismatch()
    {
        var authService = new AuthService();
        var vm = new RegisterViewModel(authService)
        {
            FullName = "Rose Ndongo",
            Phone = "+237 6 77 00 00 00",
            Email = "rose.ndongo@mail.com",
            Password = "password1",
            ConfirmPassword = "password2",
            AcceptTerms = true
        };

        Assert.True(vm.IsEmailValid);
        Assert.False(vm.IsConfirmPasswordValid);
        Assert.True(vm.HasConfirmPasswordError);
        Assert.False(vm.CanSubmit);
    }

    [Fact]
    public void RegisterViewModel_AcceptsValidForm()
    {
        var authService = new AuthService();
        var vm = new RegisterViewModel(authService)
        {
            FullName = "Rose Ndongo",
            Phone = "+237 6 77 00 00 00",
            Email = "rose.ndongo@mail.com",
            Password = "ValidPassword1",
            ConfirmPassword = "ValidPassword1",
            AcceptTerms = true
        };

        Assert.True(vm.IsFullNameValid);
        Assert.True(vm.IsPhoneValid);
        Assert.True(vm.IsEmailValid);
        Assert.True(vm.IsPasswordValid);
        Assert.True(vm.IsConfirmPasswordValid);
        Assert.True(vm.CanSubmit);
    }

    [Fact]
    public async Task SearchViewModel_DetectsNoResultsForZzz()
    {
        var clientService = new ClientService();
        var orderService = new OrderService();
        var vm = new SearchViewModel(clientService, orderService)
        {
            Query = "Zzz"
        };

        await vm.SearchAsync();
        Assert.Empty(vm.ClientResults);
        Assert.Empty(vm.OrderResults);
        Assert.True(vm.HasNoResults);
    }
}
