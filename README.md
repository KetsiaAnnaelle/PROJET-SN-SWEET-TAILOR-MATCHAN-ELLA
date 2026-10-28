#  Sweet Tailor — Application Mobile Native en .NET MAUI (XAML)

> **"Chaque mesure compte."**  
> Application mobile native Android développée en **.NET MAUI + XAML + C# (Architecture MVVM)**, reproduisant les écrans, les composants natifs, le design system et les comportements d'état de la maquette officielle.

---

##  Architecture & Organisation du Projet

Le projet respecte scrupuleusement le patron d'architecture **MVVM (Model-View-ViewModel)** et l'inversion de contrôle (IoC / Dependency Injection) :

```
SweetTailor/
├── SweetTailor.slnx                         # Solution .NET moderne
├── Design/                                  # Maquettes de référence (PDF, SVG vectoriels, HTML)
│   ├── sweet-tailor-ecrans.pdf              # 24 écrans de référence
│   └── svg/figma-svg/                       # 24 écrans SVG vectoriels extraits
│
├── src/
│   ├── SweetTailor.Api/                     # API REST ASP.NET Core et stockage SQLite local
│   ├── SweetTailor.Core/                    # Logique métier, Modèles & ViewModels (Multi-target net8.0;net10.0)
│   │   ├── Models/                          # Entités métier
│   │   │   ├── Client.cs                    # Client, catégorie, mensurations, historique
│   │   │   ├── ClientCategory.cs            # Femme, Homme, Enfant
│   │   │   ├── MeasurementItem.cs           # Haut du corps, Bas, Longueurs
│   │   │   ├── Order.cs                     # Commande, acomptes FCFA, étapes d'avancement
│   │   │   ├── OrderStatus.cs               # Reçue, Couture, Essayage, Prête, Livrée
│   │   │   ├── CatalogModel.cs              # Modèles de catalogue, styles, favoris
│   │   │   └── ClientNote.cs                # Notes d'atelier et alertes importantes
│   │   ├── Services/                        # Services métier et client HTTP de l'API
│   │   │   ├── IDemoModeService.cs          # Contrôleur d'états de démo pour la soutenance orale
│   │   │   ├── IClientService.cs            # CRUD clients, recherche, notes et mensurations
│   │   │   ├── IOrderService.cs             # CRUD commandes & machine à états
│   │   │   ├── IAppDataStore.cs              # Stockage JSON utilisé par les tests de services
│   │   │   ├── ICatalogService.cs           # Catalogue & favoris
│   │   │   ├── IMeasurementService.cs       # Validation et sauvegarde mensurations
│   │   │   ├── IAuthService.cs              # Connexion, inscription, mot de passe
│   │   │   └── MockDataSeeder.cs            # Données de démo avec dates dynamiques relatives
│   │   └── ViewModels/                      # ViewModels CommunityToolkit.Mvvm
│   │       ├── BaseViewModel.cs             # Gestion IsBusy, Titres, Toast notifications
│   │       ├── SplashViewModel.cs           # Écran de lancement
│   │       ├── OnboardingViewModel.cs       # Carrousel 3 volets & navigation
│   │       ├── LoginViewModel.cs            # Connexion & validation
│   │       ├── ForgotPasswordViewModel.cs   # Récupération de code SMS/Email
│   │       ├── RegisterViewModel.cs         # Inscription avec validation live des champs
│   │       ├── ClientsViewModel.cs          # 4 états : Normal, Chargement, Vide, Erreur NET-01
│   │       ├── ClientDetailViewModel.cs     # 3 onglets : Mesures, Commandes, Notes
│   │       ├── ClientFormViewModel.cs       # Nouveau client & sélection genre
│   │       ├── MeasurementsViewModel.cs     # Saisie 14 mesures anatomiques en cm
│   │       ├── OrdersViewModel.cs           # Commandes réparties par semaines
│   │       ├── OrderDetailViewModel.cs      # Stepper d'avancement & jauge d'acompte
│   │       ├── NewOrderViewModel.cs         # Formulaire nouvelle commande
│   │       ├── CatalogViewModel.cs          # Grille 2 colonnes et formulaire d'ajout de modèle
│   │       ├── CatalogDetailViewModel.cs    # Fiche modèle & mesures requises
│   │       ├── SearchViewModel.cs           # Recherche live & historique
│   │       └── ProfileViewModel.cs          # Profil atelier & commandes de démonstration
│   │
│   ├── SweetTailor.App/                     # Application Mobile Native .NET MAUI (XAML)
│   │   ├── App.xaml / App.xaml.cs           # Déclaration des ressources globales et convertisseurs
│   │   ├── AppShell.xaml / AppShell.xaml.cs # Shell de navigation, TabBar 4 onglets, routes
│   │   ├── MauiProgram.cs                   # Injection de dépendances et configuration polices
│   │   ├── Controls/                        # Composants natifs réutilisables
│   │   │   ├── TapeMeasure.xaml             # Mètre ruban jaune avec GraphicsView IDrawable
│   │   │   ├── AppButton.xaml               # Bouton (Primary, Accent, Ghost, Danger, Busy, Disabled)
│   │   │   ├── AppEntry.xaml                # Champ à libellé flottant (Normal, Actif, Valide, Erreur)
│   │   │   ├── SearchBox.xaml               # Barre de recherche avec loupe et bouton clear ✕
│   │   │   ├── FilterChip.xaml              # Puces de filtrage horizontales sélectionnables
│   │   │   ├── StatusPill.xaml              # Pastilles des 5 statuts métier
│   │   │   ├── AvatarInitials.xaml          # Avatars circulaires avec initiales et 4 palettes
│   │   │   ├── SegmentedTabs.xaml           # Barre d'onglets en pilule (3 options)
│   │   │   ├── OrderRow.xaml                # Ligne de commande avec date badge et statut
│   │   │   ├── ClientRow.xaml               # Ligne de client avec avatar et commande en cours
│   │   │   ├── MeasureRow.xaml              # Ligne de mensuration en cm avec mise en valeur
│   │   │   ├── OrderStepper.xaml            # Timeline verticale 4 étapes (Done, Now, Todo)
│   │   │   ├── ModelCard.xaml               # Carte modèle catalogue avec cœur favori interactif
│   │   │   ├── SkeletonList.xaml            # Squelette de chargement statique avec jauge jaune
│   │   │   ├── EmptyStateView.xaml          # Vue d'état vide avec CTA
│   │   │   ├── ErrorStateView.xaml          # Vue d'erreur réseau NET-01 avec bouton Réessayer
│   │   │   └── ToastBanner.xaml             # Notification toast (Succès, Erreur, Info)
│   │   └── Views/                           # Pages XAML de l'application
│   │       ├── SplashPage.xaml              # Écran 01 : Splash Screen (transition rapide)
│   │       ├── OnboardingPage.xaml          # Écrans 02, 03, 04 : Onboarding 3 volets
│   │       ├── LoginPage.xaml               # Écran 05 : Connexion
│   │       ├── ForgotPasswordPage.xaml      # Écran 06 : Mot de passe oublié
│   │       ├── RegisterPage.xaml            # Écran 07 : Inscription & validation
│   │       ├── ClientsPage.xaml             # Écran 08, 09, 10, 11 : Liste des clients & 4 états
│   │       ├── ClientDetailPage.xaml        # Écrans 12, 13, 14 : Fiche client & 3 onglets
│   │       ├── MeasurementsPage.xaml        # Écran 15 : Toutes les mesures anatomiques
│   │       ├── ClientFormPage.xaml          # Écran 11bis : Formulaire Nouveau client
│   │       ├── OrdersPage.xaml              # Écran 16 : Commandes par échéance
│   │       ├── NewOrderPage.xaml            # Écran 17 : Formulaire Nouvelle commande
│   │       ├── OrderDetailPage.xaml         # Écran 19 : Détail d'une commande & Stepper
│   │       ├── CatalogPage.xaml             # Écran 20 : Catalogue (grille 2 colonnes Pinterest)
│   │       ├── CatalogModelFormPage.xaml    # Ajout d'un modèle au catalogue
│   │       ├── CatalogDetailPage.xaml       # Écran 21 : Fiche modèle
│   │       ├── SearchPage.xaml              # Écrans 16bis, 18 : Recherche globale & résultats
│   │       ├── ProfilePage.xaml             # Écran 18 : Profil atelier & Panneau Mode Démo
│   │       └── ComponentsCharterPage.xaml   # Écran 24 : Matrice des composants & états
│   │
│   └── SweetTailor.Tests/                   # Suite de tests unitaires xUnit
│       └── ServicesTests.cs                 # Tests unitaires services, CRUD et ViewModels
```

---

## 💾 Backend, API et persistance

Les données clients et commandes sont cloisonnées par compte : les opérations de création, lecture, modification et suppression (CRUD), ainsi que les notes, mensurations, recherches et statistiques, ne portent que sur les données du compte connecté. Les favoris du catalogue sont également propres à chaque compte. Les anciennes fiches sans propriétaire ne sont pas exposées aux nouveaux comptes.

`SweetTailor.Api` est le backend ASP.NET Core REST. Il persiste les comptes, clients, mensurations, notes, commandes et favoris du catalogue dans `src/SweetTailor.Api/App_Data/sweet-tailor.db` (SQLite, clés étrangères, index de recherche, WAL). Les mots de passe sont hachés avec PBKDF2 ; les routes métier demandent un jeton Bearer obtenu après connexion.

Les photos de commande et de modèles sont stockées côté serveur et accessibles uniquement au propriétaire connecté. L’API accepte les images JPEG, PNG et WebP jusqu’à 5 Mo ; les modèles ajoutés au catalogue comprennent un nom, une catégorie de style, un public cible (femme, homme ou enfant), une description et une photo. Une commande ne peut passer d’« Essayage » à « Prête » que si son solde est réglé.

Les comptes, clients et commandes démarrent vides : créez un compte depuis **Inscription**, puis connectez-vous. Le catalogue de démonstration est initialisé automatiquement. L’onboarding s’affiche à la première ouverture et reste accessible depuis la connexion et le profil. Vérifiez le backend sur `http://127.0.0.1:5080/api/health`.

Les ViewModels MAUI appellent l'API via des services HTTP typés. L'adresse par défaut est `http://127.0.0.1:5080/` pour Windows et `http://10.0.2.2:5080/` pour l'émulateur Android. La valeur `sweet-tailor-api-url` dans les préférences de l'application permet de la remplacer. Sur un téléphone physique, indiquez l'adresse IP locale du PC dans cette préférence et autorisez le port 5080 dans le pare-feu.

### Lancement local

Dans deux terminaux à la racine du dépôt :

```powershell
# Terminal 1 : backend REST
dotnet run --project .\src\SweetTailor.Api\SweetTailor.Api.csproj

# Terminal 2 : frontend MAUI Windows
dotnet run --project .\src\SweetTailor.App\SweetTailor.App.csproj --framework net10.0-windows10.0.19041.0 --property:BuildingForDesktop=true
```

Pour Android, lancer l'API de la même manière puis démarrer l'application avec `dotnet run --project .\src\SweetTailor.App\SweetTailor.App.csproj --framework net10.0-android --property:BuildingForMobile=true`. Il faut un JDK et Android SDK configurés dans Visual Studio ou via `JavaSdkDirectory` et `AndroidSdkDirectory`. Le transport HTTP et le trafic clair Android sont uniquement configurés pour le développement local ; le déploiement doit utiliser HTTPS et une configuration de secrets adaptée. L'envoi de codes de réinitialisation de mot de passe reste désactivé tant qu'un fournisseur SMS/e-mail n'est pas configuré ; l'API renvoie alors une erreur explicite au lieu de prétendre qu'un message a été envoyé.

---

##  Charte Graphique & Tokens Officiels

| Token | Couleur | Valeur Hex | Utilisation |
|---|---|---|---|
| **Indigo** | Primary | `#26306B` | Boutons principaux, titres de marque, étapes terminées |
| **IndigoDark** | Pressed | `#1B2352` | État pressé des boutons |
| **Ink** | Texte principal | `#14173A` | Texte principal, titres d'écrans |
| **Rose** | Accent CTA | `#E8607F` | Boutons d'action rose, FAB, échéance urgente, nœud du logo |
| **RoseSoft** | Fond rose | `#FCE4EA` | Fond d'accent, badges doux, pastille essayage |
| **Tape** | Mètre ruban | `#F4C95D` | Fil continu du logo « S », graduations mètre ruban |
| **TapeSoft** | Fond jaune | `#FDF3D6` | Fond panneau de démo, pastille couture |
| **Chalk** | Fond de page | `#F6F5FA` | Fond général de l'application |
| **Mist** | Neutre | `#E4E5F2` | Séparateurs, bordures douces, pastille reçue |
| **Line** | Bordure | `#E8E9F3` | Bordures de cartes et cellules |
| **Muted** | Texte secondaire | `#6B7094` | Sous-titres, dates, texte d'aide |
| **Success / Soft** | Vert | `#2E9E6B` / `#DDF3E8` | Prête à retirer, validation valide, toasts succès |
| **Error / Soft** | Rouge | `#D93A4A` / `#FDE8EA` | Erreurs de formulaire, état réseau NET-01 |
| **Disabled** | Grisé | `#C9CCEA` (btn) / `#EEEFF6` (champ) | Contrôles désactivés |

### Polices Google Fonts intégrées
- **Bricolage Grotesque ExtraBold** (`BricolageExtraBold`) pour tous les titres.
- **DM Sans** Regular, Medium, SemiBold, Bold (`DMSans`, `DMSansMedium`, `DMSansSemiBold`, `DMSansBold`) pour le texte.

---

##  Charte des composants (Écran 24)

Cliquez sur **« 📐 Voir la charte des composants »** dans le profil pour ouvrir l'écran vitrine :
   - Présente tous les composants natifs côte à côte dans leurs états respectifs :
     - `AppButton` (Normal, Accent, Ghost, Danger, Désactivé, IsBusy avec ActivityIndicator).
     - `AppEntry` (Normal, Focus/Saisie, Valide avec check vert, Erreur avec halo rouge, Mot de passe avec bouton œil, Désactivé).
     - `StatusPill` (les 5 statuts : Reçue, Couture, Essayage, Prête, Livrée).
     - `FilterChip` (sélectionné vs normal).
     - `ToastBanner` interactif avec 3 boutons de déclenchement direct (Succès, Erreur, Info).
     - `OrderStepper`, `MeasureRow`, `TapeMeasure`.

Le panneau de simulation du profil ne réinitialise pas la base SQLite : les clients et commandes de l'application active sont gérés par l'API.

---

##  Compilation & Exécution

### 1. Compilation avec .NET CLI
```powershell
# Naviguer dans le dossier du projet
cd "C:\Users\HP ELITEBOOK G8\.gemini\antigravity\scratch\SweetTailor"

# Compiler réellement l'application Android et ses pages XAML (JDK 17 + SDK Android requis)
dotnet build .\src\SweetTailor.App\SweetTailor.App.csproj -p:BuildingForMobile=true -f net10.0-android

# Publier l'application Desktop MAUI Windows autonome
dotnet publish .\src\SweetTailor.App\SweetTailor.App.csproj -p:BuildingForDesktop=true -f net10.0-windows10.0.19041.0 -r win-x64 -c Release

# Compiler le reste de la solution
dotnet build

# Exécuter les tests unitaires automatisés
dotnet test
```

### 2. Exécution sur Émulateur Android (ou terminal physique)
Dans **Visual Studio Code** avec l'extension **.NET MAUI** :
1. Ouvrez le dossier `SweetTailor`.
2. Appuyez sur `Ctrl+Shift+P` et choisissez **.NET MAUI: Pick Android Device**.
3. Sélectionnez votre émulateur Android (ex: `pixel_5 - api 34`).
4. Appuyez sur `F5` pour compiler, déployer et lancer l'application en mode débogage.

En ligne de commande :
```powershell
dotnet build -t:Run -f net10.0-android -p:BuildingForMobile=true .\src\SweetTailor.App\SweetTailor.App.csproj
```

### 3. Exécution du vrai frontend MAUI Desktop dans VS Code
1. Ouvrez le dossier `SweetTailor` dans VS Code.
2. Dans **Exécuter et déboguer** (`Ctrl+Shift+D`), choisissez **Sweet Tailor (MAUI Desktop)**.
3. Appuyez sur `F5`. VS Code démarre l'API si elle n'est pas déjà disponible, publie la cible Windows MAUI en mode Debug puis ouvre sa fenêtre.

Le profil **Lancer Sweet Tailor (Simulateur Mobile)** démarre l'ancien simulateur WPF ; choisissez le profil **MAUI Desktop** pour voir les pages XAML de l'application. L'exécutable publié en Release se trouve dans `src\SweetTailor.App\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\SweetTailor.App.exe`.

---

##  Tests Unitaires Automatisés (xUnit)
La suite de tests unitaires valide :
- La cohérence des données mock et des calculs d'acomptes/solde FCFA.
- Le cycle de vie de la commande et la transition entre étapes.
- Les filtres et les favoris du catalogue.
- La validation des formulaires d'inscription (format email, longueur et chiffres du mot de passe, concordance des mots de passe).
- La gestion des résultats vides de la recherche.
