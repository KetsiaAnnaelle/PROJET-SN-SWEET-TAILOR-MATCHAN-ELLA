using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public static class MockDataSeeder
{
    public static List<Client> GetInitialClients()
    {
        var nadege = new Client
        {
            Id = 1,
            FullName = "Nadège Mbarga",
            PhoneNumber = "+237 6 77 00 00 00",
            Address = "Bonamoussadi, Douala",
            Category = ClientCategory.Femme,
            NotesSummary = "Préfère les coupes ajustées",
            AvatarInitials = "NM",
            AvatarColorClass = "a1",
            CurrentOrderSummary = "Robe de soirée · pour le 8 oct.",
            CurrentOrderStatus = OrderStatus.Essayage,
            ActiveOrdersCount = 1,
            TotalOrdersCount = 3,
            CreatedAt = new DateTime(2026, 9, 12),
            Measurements = new List<MeasurementItem>
            {
                // Haut du corps
                new() { Name = "Tour de cou", Value = 34, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Carrure dos", Value = 38, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Longueur d'épaule", Value = 12, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de poitrine", Value = 92, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Hauteur de poitrine", Value = 26, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de taille", Value = 74, Unit = "cm", Category = MeasurementCategory.HautDuCorps, IsHighlighted = true },
                new() { Name = "Longueur de manche", Value = 58, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de bras", Value = 28, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                // Bas
                new() { Name = "Tour de hanches", Value = 98, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Tour de cuisse", Value = 56, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Entrejambe", Value = 76, Unit = "cm", Category = MeasurementCategory.Bas },
                // Longueurs
                new() { Name = "Longueur robe", Value = 112, Unit = "cm", Category = MeasurementCategory.Longueurs },
                new() { Name = "Longueur jupe", Value = 62, Unit = "cm", Category = MeasurementCategory.Longueurs },
                new() { Name = "Hauteur totale", Value = 168, Unit = "cm", Category = MeasurementCategory.Longueurs }
            },
            Notes = new List<ClientNote>
            {
                new() { Id = 1, ClientId = 1, Date = new DateTime(2026, 9, 12), Content = "Préfère les coupes ajustées et les manches 3/4.", IsImportant = true },
                new() { Id = 2, ClientId = 1, Date = new DateTime(2026, 9, 28), Content = "Ajouter 2 cm de marge à la taille pour la robe de soirée.", IsImportant = false },
                new() { Id = 3, ClientId = 1, Date = new DateTime(2026, 8, 14), Content = "Aime le satin et les couleurs sombres. Éviter le rouge.", IsImportant = false }
            }
        };

        var christelle = new Client
        {
            Id = 2,
            FullName = "Christelle Tchoua",
            PhoneNumber = "+237 6 99 12 34 56",
            Address = "Akwa, Douala",
            Category = ClientCategory.Femme,
            NotesSummary = "Aime les motifs vifs",
            AvatarInitials = "CT",
            AvatarColorClass = "a2",
            CurrentOrderSummary = "Ensemble pagne · pour le 11 oct.",
            CurrentOrderStatus = OrderStatus.Couture,
            ActiveOrdersCount = 1,
            TotalOrdersCount = 2,
            CreatedAt = new DateTime(2026, 9, 18),
            Measurements = new List<MeasurementItem>
            {
                new() { Name = "Tour de cou", Value = 33, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Carrure dos", Value = 36, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Longueur d'épaule", Value = 11, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de poitrine", Value = 88, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Hauteur de poitrine", Value = 25, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de taille", Value = 70, Unit = "cm", Category = MeasurementCategory.HautDuCorps, IsHighlighted = true },
                new() { Name = "Longueur de manche", Value = 56, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de bras", Value = 26, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de hanches", Value = 94, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Tour de cuisse", Value = 54, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Entrejambe", Value = 74, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Longueur jupe", Value = 65, Unit = "cm", Category = MeasurementCategory.Longueurs },
                new() { Name = "Hauteur totale", Value = 164, Unit = "cm", Category = MeasurementCategory.Longueurs }
            },
            Notes = new List<ClientNote>
            {
                new() { Id = 10, ClientId = 2, Date = new DateTime(2026, 9, 18), Content = "Préfère les motifs wax vifs et colorés (jaune/bleu).", IsImportant = true },
                new() { Id = 11, ClientId = 2, Date = new DateTime(2026, 10, 2), Content = "Prévoir fente d'aisance sur le côté de la jupe.", IsImportant = false }
            }
        };

        var patrick = new Client
        {
            Id = 3,
            FullName = "Patrick Essomba",
            PhoneNumber = "+237 6 55 44 33 22",
            Address = "Bastos, Yaoundé",
            Category = ClientCategory.Homme,
            NotesSummary = "Costumes cintrés uniquement",
            AvatarInitials = "PE",
            AvatarColorClass = "a3",
            CurrentOrderSummary = "Costume 3 pièces · pour le 14 oct.",
            CurrentOrderStatus = OrderStatus.Recue,
            ActiveOrdersCount = 1,
            TotalOrdersCount = 1,
            CreatedAt = new DateTime(2026, 9, 25),
            Measurements = new List<MeasurementItem>
            {
                new() { Name = "Tour de cou", Value = 41, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Carrure dos", Value = 46, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Longueur d'épaule", Value = 16, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de poitrine", Value = 104, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Hauteur de poitrine", Value = 28, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de taille", Value = 86, Unit = "cm", Category = MeasurementCategory.HautDuCorps, IsHighlighted = true },
                new() { Name = "Longueur de manche", Value = 64, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de bras", Value = 34, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de bassin", Value = 100, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Tour de cuisse", Value = 60, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Entrejambe", Value = 82, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Longueur veste", Value = 78, Unit = "cm", Category = MeasurementCategory.Longueurs },
                new() { Name = "Longueur pantalon", Value = 105, Unit = "cm", Category = MeasurementCategory.Longueurs },
                new() { Name = "Hauteur totale", Value = 182, Unit = "cm", Category = MeasurementCategory.Longueurs }
            },
            Notes = new List<ClientNote>
            {
                new() { Id = 20, ClientId = 3, Date = new DateTime(2026, 9, 25), Content = "Costumes cintrés uniquement (coupe italienne slim). Deux fentes au dos.", IsImportant = true },
                new() { Id = 21, ClientId = 3, Date = new DateTime(2026, 10, 3), Content = "Doublure en soie satinée bleu nuit.", IsImportant = false }
            }
        };

        var armelle = new Client
        {
            Id = 4,
            FullName = "Armelle Fotso",
            PhoneNumber = "+237 6 70 88 99 00",
            Address = "Makepe, Douala",
            Category = ClientCategory.Femme,
            NotesSummary = "Kaba traditionnel cérémonie",
            AvatarInitials = "AF",
            AvatarColorClass = "a4",
            CurrentOrderSummary = "Kaba · prête à retirer",
            CurrentOrderStatus = OrderStatus.Prete,
            ActiveOrdersCount = 0,
            TotalOrdersCount = 4,
            CreatedAt = new DateTime(2026, 8, 30),
            Measurements = new List<MeasurementItem>
            {
                new() { Name = "Tour de cou", Value = 36, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Carrure dos", Value = 40, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Longueur d'épaule", Value = 13, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de poitrine", Value = 96, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Hauteur de poitrine", Value = 27, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de taille", Value = 80, Unit = "cm", Category = MeasurementCategory.HautDuCorps, IsHighlighted = true },
                new() { Name = "Longueur de manche", Value = 59, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de bras", Value = 30, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de hanches", Value = 106, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Tour de cuisse", Value = 60, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Entrejambe", Value = 75, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Longueur kaba", Value = 135, Unit = "cm", Category = MeasurementCategory.Longueurs },
                new() { Name = "Hauteur totale", Value = 166, Unit = "cm", Category = MeasurementCategory.Longueurs }
            },
            Notes = new List<ClientNote>
            {
                new() { Id = 30, ClientId = 4, Date = new DateTime(2026, 8, 30), Content = "Kaba traditionnel cérémonie. Broderie dorée au col et manches.", IsImportant = true }
            }
        };

        var joel = new Client
        {
            Id = 5,
            FullName = "Joël Mbappé",
            PhoneNumber = "+237 6 90 11 22 33",
            Address = "Deido, Douala",
            Category = ClientCategory.Homme,
            NotesSummary = "Chemise lin col mao",
            AvatarInitials = "JM",
            AvatarColorClass = "a3",
            CurrentOrderSummary = "1 commande · livrée",
            CurrentOrderStatus = OrderStatus.Livree,
            ActiveOrdersCount = 0,
            TotalOrdersCount = 1,
            CreatedAt = new DateTime(2026, 7, 10),
            Measurements = new List<MeasurementItem>
            {
                new() { Name = "Tour de cou", Value = 40, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Carrure dos", Value = 44, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Longueur d'épaule", Value = 15, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de poitrine", Value = 98, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Hauteur de poitrine", Value = 26, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de taille", Value = 82, Unit = "cm", Category = MeasurementCategory.HautDuCorps, IsHighlighted = true },
                new() { Name = "Longueur de manche", Value = 62, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de bras", Value = 32, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de bassin", Value = 96, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Entrejambe", Value = 80, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Longueur chemise", Value = 75, Unit = "cm", Category = MeasurementCategory.Longueurs },
                new() { Name = "Hauteur totale", Value = 176, Unit = "cm", Category = MeasurementCategory.Longueurs }
            },
            Notes = new List<ClientNote>
            {
                new() { Id = 40, ClientId = 5, Date = new DateTime(2026, 7, 10), Content = "Chemise lin col mao blanche. Boutons nacrés discrets.", IsImportant = false }
            }
        };

        var sandrine = new Client
        {
            Id = 6,
            FullName = "Sandrine Kamga",
            PhoneNumber = "+237 6 74 55 66 77",
            Address = "Kotto, Douala",
            Category = ClientCategory.Femme,
            NotesSummary = "Mariage civil du 24 oct.",
            AvatarInitials = "SK",
            AvatarColorClass = "a1",
            CurrentOrderSummary = "Robe de mariée · pour le 17 oct.",
            CurrentOrderStatus = OrderStatus.Couture,
            ActiveOrdersCount = 1,
            TotalOrdersCount = 1,
            CreatedAt = new DateTime(2026, 9, 15),
            Measurements = new List<MeasurementItem>
            {
                new() { Name = "Tour de cou", Value = 34, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Carrure dos", Value = 37, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Longueur d'épaule", Value = 11.5, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de poitrine", Value = 90, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Hauteur de poitrine", Value = 25, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de taille", Value = 72, Unit = "cm", Category = MeasurementCategory.HautDuCorps, IsHighlighted = true },
                new() { Name = "Longueur de manche", Value = 57, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de bras", Value = 27, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                new() { Name = "Tour de hanches", Value = 96, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Tour de cuisse", Value = 55, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Entrejambe", Value = 77, Unit = "cm", Category = MeasurementCategory.Bas },
                new() { Name = "Longueur robe mariée", Value = 145, Unit = "cm", Category = MeasurementCategory.Longueurs },
                new() { Name = "Traîne", Value = 180, Unit = "cm", Category = MeasurementCategory.Longueurs },
                new() { Name = "Hauteur totale", Value = 170, Unit = "cm", Category = MeasurementCategory.Longueurs }
            },
            Notes = new List<ClientNote>
            {
                new() { Id = 50, ClientId = 6, Date = new DateTime(2026, 9, 15), Content = "Mariage civil du 24 oct. Dentelle perlée mikado blanc soie.", IsImportant = true },
                new() { Id = 51, ClientId = 6, Date = new DateTime(2026, 10, 5), Content = "Essayage final impératif prévu pour le 17 oct.", IsImportant = false }
            }
        };

        return new List<Client> { nadege, christelle, patrick, armelle, joel, sandrine };
    }

    public static List<Order> GetInitialOrders()
    {
        var fr = new System.Globalization.CultureInfo("fr-FR");
        return new List<Order>
        {
            new()
            {
                Id = 142,
                OrderNumber = "#0142",
                ClientId = 1,
                ClientName = "Nadège Mbarga",
                ClientPhone = "+237 6 77 00 00 00",
                ModelName = "Robe de soirée longue",
                Fabric = "Satin bleu nuit",
                TotalPrice = 45000,
                AdvancePayment = 20000,
                DueDate = DateTime.Today.AddDays(5),
                DueDateLabel = $"Dans 5 jours · {DateTime.Today.AddDays(5).ToString("d MMM", fr)}",
                IsHotDueDate = true,
                Status = OrderStatus.Essayage,
                SvgAsset = "dress",
                ImagePath = "robe_soiree.jpg",
                GradientClass = "g-ind",
                Steps = new List<OrderStep>
                {
                    new() { Title = "Commande reçue", Subtitle = DateTime.Today.AddDays(-10).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Coupe et couture", Subtitle = DateTime.Today.AddDays(-6).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Essayage client", Subtitle = "En cours", State = StepState.Now },
                    new() { Title = "Prête à retirer", Subtitle = string.Empty, State = StepState.Todo }
                }
            },
            new()
            {
                Id = 143,
                OrderNumber = "#0143",
                ClientId = 2,
                ClientName = "Christelle Tchoua",
                ClientPhone = "+237 6 99 12 34 56",
                ModelName = "Ensemble pagne",
                Fabric = "Wax hollandais",
                TotalPrice = 38000,
                AdvancePayment = 25000,
                DueDate = DateTime.Today.AddDays(8),
                DueDateLabel = $"Dans 8 jours · {DateTime.Today.AddDays(8).ToString("d MMM", fr)}",
                IsHotDueDate = false,
                Status = OrderStatus.Couture,
                SvgAsset = "kaba",
                ImagePath = "jupe_pagne.jpg",
                GradientClass = "g-tape",
                Steps = new List<OrderStep>
                {
                    new() { Title = "Commande reçue", Subtitle = DateTime.Today.AddDays(-8).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Coupe et couture", Subtitle = "En cours", State = StepState.Now },
                    new() { Title = "Essayage client", Subtitle = string.Empty, State = StepState.Todo },
                    new() { Title = "Prête à retirer", Subtitle = string.Empty, State = StepState.Todo }
                }
            },
            new()
            {
                Id = 144,
                OrderNumber = "#0144",
                ClientId = 3,
                ClientName = "Patrick Essomba",
                ClientPhone = "+237 6 55 44 33 22",
                ModelName = "Costume 3 pièces",
                Fabric = "Laine italienne gris anthracite",
                TotalPrice = 85000,
                AdvancePayment = 40000,
                DueDate = DateTime.Today.AddDays(11),
                DueDateLabel = $"Dans 11 jours · {DateTime.Today.AddDays(11).ToString("d MMM", fr)}",
                IsHotDueDate = false,
                Status = OrderStatus.Recue,
                SvgAsset = "suit",
                ImagePath = "costume_homme.jpg",
                GradientClass = "g-mist",
                Steps = new List<OrderStep>
                {
                    new() { Title = "Commande reçue", Subtitle = DateTime.Today.AddDays(-2).ToString("d MMM", fr), State = StepState.Now },
                    new() { Title = "Coupe et couture", Subtitle = string.Empty, State = StepState.Todo },
                    new() { Title = "Essayage client", Subtitle = string.Empty, State = StepState.Todo },
                    new() { Title = "Prête à retirer", Subtitle = string.Empty, State = StepState.Todo }
                }
            },
            new()
            {
                Id = 140,
                OrderNumber = "#0140",
                ClientId = 4,
                ClientName = "Armelle Fotso",
                ClientPhone = "+237 6 70 88 99 00",
                ModelName = "Kaba traditionnel",
                Fabric = "Bazin riche",
                TotalPrice = 25000,
                AdvancePayment = 25000,
                DueDate = DateTime.Today.AddDays(1),
                DueDateLabel = $"Demain · {DateTime.Today.AddDays(1).ToString("d MMM", fr)}",
                IsHotDueDate = true,
                Status = OrderStatus.Prete,
                SvgAsset = "kaba",
                ImagePath = "kaba_africain.jpg",
                GradientClass = "g-rose",
                Steps = new List<OrderStep>
                {
                    new() { Title = "Commande reçue", Subtitle = DateTime.Today.AddDays(-15).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Coupe et couture", Subtitle = DateTime.Today.AddDays(-7).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Essayage client", Subtitle = DateTime.Today.AddDays(-2).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Prête à retirer", Subtitle = "Prête", State = StepState.Now }
                }
            },
            new()
            {
                Id = 139,
                OrderNumber = "#0139",
                ClientId = 5,
                ClientName = "Joël Nguemo",
                ClientPhone = "+237 6 81 22 33 44",
                ModelName = "Chemise col mao",
                Fabric = "Lin blanc",
                TotalPrice = 18000,
                AdvancePayment = 18000,
                DueDate = DateTime.Today.AddDays(-5),
                DueDateLabel = $"Livrée il y a 5 jours · payée",
                IsHotDueDate = false,
                Status = OrderStatus.Livree,
                SvgAsset = "suit",
                ImagePath = "costume_homme.jpg",
                GradientClass = "g-tape",
                Steps = new List<OrderStep>
                {
                    new() { Title = "Commande reçue", Subtitle = DateTime.Today.AddDays(-20).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Coupe et couture", Subtitle = DateTime.Today.AddDays(-14).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Essayage client", Subtitle = DateTime.Today.AddDays(-8).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Prête à retirer", Subtitle = "Livrée", State = StepState.Done }
                }
            },
            new()
            {
                Id = 145,
                OrderNumber = "#0145",
                ClientId = 6,
                ClientName = "Sandrine Kamga",
                ClientPhone = "+237 6 74 55 66 77",
                ModelName = "Robe de mariée",
                Fabric = "Dentelle perlée et mikado blanc soie",
                TotalPrice = 150000,
                AdvancePayment = 80000,
                DueDate = DateTime.Today.AddDays(14),
                DueDateLabel = $"Dans 14 jours · {DateTime.Today.AddDays(14).ToString("d MMM", fr)}",
                IsHotDueDate = false,
                Status = OrderStatus.Couture,
                SvgAsset = "gown",
                ImagePath = "robe_mariee.jpg",
                GradientClass = "g-rose",
                Steps = new List<OrderStep>
                {
                    new() { Title = "Commande reçue", Subtitle = DateTime.Today.AddDays(-12).ToString("d MMM", fr), State = StepState.Done },
                    new() { Title = "Coupe et couture", Subtitle = "En cours", State = StepState.Now },
                    new() { Title = "Essayage client", Subtitle = string.Empty, State = StepState.Todo },
                    new() { Title = "Prête à retirer", Subtitle = string.Empty, State = StepState.Todo }
                }
            }
        };
    }

    public static List<CatalogModel> GetInitialCatalog()
    {
        return new List<CatalogModel>
        {
            new()
            {
                Id = 1,
                Title = "Robe sirène en satin",
                Category = "Mariage",
                Subtitle = "Coupe ajustée, évasée en bas",
                SvgAsset = "gown",
                ImagePath = "robe_soiree.jpg",
                SvgFillColor = "#26306B",
                GradientClass = "g-rose",
                IsFavorite = false,
                CardHeight = 210,
                RequiredMeasurements = new List<string> { "Poitrine", "Taille", "Hanches", "Longueur robe" },
                SimilarModelIds = new List<int> { 3, 5, 6 }
            },
            new()
            {
                Id = 2,
                Title = "Ensemble pagne",
                Category = "Pagne",
                Subtitle = "Jupe et haut stylisé",
                SvgAsset = "kaba",
                ImagePath = "jupe_pagne.jpg",
                SvgFillColor = "#26306B",
                GradientClass = "g-tape",
                IsFavorite = true,
                CardHeight = 150,
                RequiredMeasurements = new List<string> { "Carrure", "Poitrine", "Taille", "Bassin" }
            },
            new()
            {
                Id = 3,
                Title = "Robe cocktail",
                Category = "Soirée",
                Subtitle = "Élégante et fluide",
                SvgAsset = "dress",
                ImagePath = "robe_cocktail.jpg",
                SvgFillColor = "#F4C95D",
                GradientClass = "g-ind",
                IsFavorite = false,
                CardHeight = 220,
                RequiredMeasurements = new List<string> { "Tour de cou", "Poitrine", "Taille", "Longueur" }
            },
            new()
            {
                Id = 4,
                Title = "Costume 3 pièces",
                Category = "Homme",
                Subtitle = "Coupe slim sur mesure",
                SvgAsset = "suit",
                ImagePath = "costume_homme.jpg",
                SvgFillColor = "#26306B",
                GradientClass = "g-mist",
                IsFavorite = false,
                CardHeight = 180,
                RequiredMeasurements = new List<string> { "Épaule", "Poitrine", "Taille", "Manche", "Entrejambe" }
            },
            new()
            {
                Id = 5,
                Title = "Jupe plissée en wax",
                Category = "Pagne",
                Subtitle = "Taille haute à volants",
                SvgAsset = "skirt",
                ImagePath = "jupe_pagne.jpg",
                SvgFillColor = "#1E7A50",
                GradientClass = "g-green",
                IsFavorite = false,
                CardHeight = 140,
                RequiredMeasurements = new List<string> { "Tour de taille", "Longueur jupe" }
            },
            new()
            {
                Id = 6,
                Title = "Kaba brodé",
                Category = "Traditionnel",
                Subtitle = "Broderie fine poitrine et manches",
                SvgAsset = "kaba",
                ImagePath = "kaba_africain.jpg",
                SvgFillColor = "#FFFFFF",
                GradientClass = "g-pink",
                IsFavorite = false,
                CardHeight = 190,
                RequiredMeasurements = new List<string> { "Carrure dos", "Poitrine", "Hauteur totale" }
            }
        };
    }
}
