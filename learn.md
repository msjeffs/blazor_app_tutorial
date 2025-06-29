# C# .NET Development Best Practices and Learning Guide

This guide covers the best practices, patterns, and concepts used in this Blazor Tic Tac Toe application, following official Microsoft documentation and industry standards.

## 📚 Table of Contents

- [C# Language Best Practices](#c-language-best-practices)
- [.NET Framework Architecture](#net-framework-architecture)
- [Blazor Framework Deep Dive](#blazor-framework-deep-dive)
- [Entity Framework Core](#entity-framework-core)
- [ASP.NET Core Identity](#aspnet-core-identity)
- [Dependency Injection](#dependency-injection)
- [Project Structure & Organization](#project-structure--organization)
- [Security Best Practices](#security-best-practices)
- [Performance Optimization](#performance-optimization)
- [Testing Strategies](#testing-strategies)
- [Code Quality & Standards](#code-quality--standards)

## 🔤 C# Language Best Practices

### Modern C# Features Used

#### 1. **Nullable Reference Types** (C# 8.0+)
```csharp
// Enable nullable reference types in project file
<Nullable>enable</Nullable>

// Explicit nullable declarations
public class ApplicationUser : IdentityUser
{
    public string? PhoneNumber { get; set; }  // Nullable
    public string Email { get; set; } = null!; // Non-nullable, initialized
}
```

**Benefits**:
- Compile-time null safety
- Reduced NullReferenceExceptions
- Better API design

#### 2. **Primary Constructors** (C# 12.0+)
```csharp
// Modern approach used in our DbContext
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    // Constructor parameter automatically becomes a field
}
```

#### 3. **Collection Expressions** (C# 12.0+)
```csharp
// Modern collection initialization
int[][] winConditions = {
    [0, 1, 2], [3, 4, 5], [6, 7, 8], // rows
    [0, 3, 6], [1, 4, 7], [2, 5, 8], // columns
    [0, 4, 8], [2, 4, 6]             // diagonals
};
```

#### 4. **Pattern Matching & Switch Expressions**
```csharp
// Used in our game result display
private string GetGameResultMessage()
{
    return currentGame?.Result switch
    {
        GameResult.UserWin => "🎉 You Won!",
        GameResult.ComputerWin => "🤖 Computer Won!",
        GameResult.Draw => "🤝 It's a Draw!",
        _ => ""
    };
}
```

#### 5. **Async/Await Best Practices**
```csharp
// Proper async method naming and implementation
public async Task<Game> CreateNewGameAsync(string userId)
{
    var game = new Game
    {
        UserId = userId,
        GameState = JsonSerializer.Serialize(new string[9]),
        Result = GameResult.InProgress
    };
    
    _context.Games.Add(game);
    await _context.SaveChangesAsync(); // Async database operation
    
    return game;
}
```

### Naming Conventions

#### **PascalCase**: Classes, Methods, Properties, Events
```csharp
public class GameService
{
    public async Task<Game> CreateNewGameAsync(string userId) { }
    public int TotalGamesPlayed { get; set; }
}
```

#### **camelCase**: Variables, Parameters, Fields
```csharp
private readonly ApplicationDbContext _context;
private string[] board = new string[9];
private bool isProcessing = false;
```

#### **UPPER_CASE**: Constants
```csharp
public const int BOARD_SIZE = 9;
private const string EMPTY_CELL = "";
```

## 🏗️ .NET Framework Architecture

### Application Structure

```
TicTacToeApp/
├── Components/          # Blazor components and pages
│   ├── Account/        # Authentication components
│   ├── Layout/         # Layout components
│   └── Pages/          # Application pages
├── Data/               # Data models and database context
├── Services/           # Business logic services
├── wwwroot/           # Static files (CSS, JS, images)
└── Program.cs         # Application entry point
```

### Dependency Injection Container

Our application uses .NET's built-in DI container:

```csharp
// Service registration in Program.cs
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Database context with proper lifetime
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Custom services
builder.Services.AddScoped<GameService>();

// Identity services
builder.Services.AddIdentityCore<ApplicationUser>(options => /* config */)
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

### Configuration System

```csharp
// appsettings.json structure
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=Data\\app.db;Cache=Shared"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}

// Accessing configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
```

## ⚡ Blazor Framework Deep Dive

### Component Architecture

#### **Component Lifecycle**
```csharp
protected override async Task OnInitializedAsync()
{
    // Called when component is first initialized
    // Good for: Loading initial data, setting up subscriptions
    await LoadUserStats();
    await StartNewGame();
}

protected override async Task OnParametersSetAsync()
{
    // Called when parameters change
    // Good for: Responding to parameter changes
}

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    // Called after component has rendered
    // Good for: JavaScript interop, DOM manipulation
}
```

#### **Server-Side Rendering (Blazor Server)**
- **Real-time UI updates** over SignalR connection
- **Server state management** - UI state lives on server
- **Low bandwidth** - Only UI changes sent to client
- **Full .NET API access** - Direct database access

```csharp
@rendermode InteractiveServer  // Enables real-time interactivity

// State changes automatically update UI
private async Task MakeMove(int position)
{
    // State change on server
    currentGame = await GameService.MakeMoveAsync(currentGame.Id, currentUser.Id, position);
    board = JsonSerializer.Deserialize<string[]>(currentGame.GameState);
    
    // UI automatically updates on client
}
```

### Event Handling

```csharp
// Click events with parameters
<button @onclick="() => MakeMove(index)">

// Conditional rendering
@if (currentGame.IsCompleted)
{
    <button class="btn btn-primary" @onclick="StartNewGame">
        🎮 Play Again
    </button>
}

// Form submissions
<form @onsubmit="HandleSubmit" @onsubmit:preventDefault="true">
```

### CSS Isolation

```css
/* TicTacToe.razor.css - Component-scoped styles */
.game-container {
    max-width: 600px;
    margin: 2rem auto;
}

/* Automatically scoped to this component only */
.board-cell:hover:not(:disabled) {
    background: #e3f2fd;
    transform: scale(1.05);
}
```

## 🗄️ Entity Framework Core

### Code-First Approach

#### **1. Model Definition**
```csharp
public class Game
{
    [Key]  // Primary key attribute
    public int Id { get; set; }
    
    [Required]  // Validation attribute
    public string UserId { get; set; } = null!;
    
    // Navigation property
    public ApplicationUser User { get; set; } = null!;
    
    public GameResult Result { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Enum for type safety
public enum GameResult
{
    InProgress = 0,
    UserWin = 1,
    ComputerWin = 2,
    Draw = 3
}
```

#### **2. DbContext Configuration**
```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Game> Games => Set<Game>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Fluent API configuration
        builder.Entity<Game>(entity =>
        {
            entity.HasOne(g => g.User)
                  .WithMany(u => u.Games)
                  .HasForeignKey(g => g.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.Property(g => g.GameState)
                  .HasMaxLength(1000);
        });
    }
}
```

#### **3. Migrations**
```bash
# Create migration
dotnet ef migrations add AddGameModel

# Apply migration
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Query Patterns

#### **Async Queries**
```csharp
// Single entity with conditions
public async Task<Game?> GetGameAsync(int gameId, string userId)
{
    return await _context.Games
        .FirstOrDefaultAsync(g => g.Id == gameId && g.UserId == userId);
}

// List with filtering and ordering
public async Task<List<Game>> GetUserGamesAsync(string userId, int pageSize = 10)
{
    return await _context.Games
        .Where(g => g.UserId == userId)
        .OrderByDescending(g => g.CreatedAt)
        .Take(pageSize)
        .ToListAsync();
}
```

#### **Change Tracking**
```csharp
// EF automatically tracks changes
var game = await _context.Games.FindAsync(gameId);
game.IsCompleted = true;
game.CompletedAt = DateTime.UtcNow;

// Single SaveChanges call for all changes
await _context.SaveChangesAsync();
```

## 🔐 ASP.NET Core Identity

### Authentication Architecture

#### **Identity Configuration**
```csharp
// Configure Identity options
builder.Services.AddIdentityCore<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;  // Simplify for demo
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();
```

#### **Custom User Model**
```csharp
public class ApplicationUser : IdentityUser
{
    // Custom properties
    public int Wins { get; set; } = 0;
    public int TotalGamesPlayed { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
```

### Authorization

#### **Component-Level Authorization**
```csharp
@attribute [Authorize]  // Require authentication for entire page

// Conditional rendering based on auth status
<AuthorizeView>
    <Authorized>
        <p>Welcome, @context.User.Identity?.Name!</p>
    </Authorized>
    <NotAuthorized>
        <p>Please log in to play.</p>
    </NotAuthorized>
</AuthorizeView>
```

#### **Service-Level Authorization**
```csharp
public async Task<Game> MakeMoveAsync(int gameId, string userId, int position)
{
    // Verify user owns the game
    var game = await GetGameAsync(gameId, userId);
    if (game == null)
        throw new InvalidOperationException("Game not found or access denied");
        
    // Business logic here...
}
```

## 💉 Dependency Injection

### Service Lifetimes

#### **Singleton** - Single instance for entire application
```csharp
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
```

#### **Scoped** - One instance per request
```csharp
builder.Services.AddScoped<GameService>();
builder.Services.AddDbContext<ApplicationDbContext>(); // Scoped by default
```

#### **Transient** - New instance every time
```csharp
builder.Services.AddTransient<ITransientService, TransientService>();
```

### Service Registration Patterns

```csharp
// Interface-based registration
public interface IGameService
{
    Task<Game> CreateNewGameAsync(string userId);
}

public class GameService : IGameService
{
    private readonly ApplicationDbContext _context;
    
    public GameService(ApplicationDbContext context)
    {
        _context = context;
    }
}

// Registration
builder.Services.AddScoped<IGameService, GameService>();
```

### Component Injection

```csharp
@inject GameService GameService
@inject UserManager<ApplicationUser> UserManager
@inject NavigationManager Navigation

// Use in component
protected override async Task OnInitializedAsync()
{
    currentUser = await UserManager.GetUserAsync(authState.User);
    currentGame = await GameService.CreateNewGameAsync(currentUser.Id);
}
```

## 📁 Project Structure & Organization

### Folder Organization

```
├── Components/
│   ├── Account/          # Identity-related components
│   │   ├── Pages/        # Login, Register, etc.
│   │   └── Shared/       # Shared account components
│   ├── Layout/           # Application layout
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Pages/            # Application pages
│       ├── Home.razor
│       ├── TicTacToe.razor
│       └── *.razor.css   # Component-scoped styles
├── Data/                 # Data layer
│   ├── Migrations/       # EF Core migrations
│   ├── ApplicationDbContext.cs
│   ├── ApplicationUser.cs
│   └── Game.cs
├── Services/             # Business logic
│   └── GameService.cs
├── wwwroot/             # Static assets
│   ├── css/
│   ├── js/
│   └── images/
└── Program.cs           # Application startup
```

### File Naming Conventions

- **Components**: PascalCase with `.razor` extension
- **CSS Files**: Component name + `.razor.css`
- **Services**: ServiceName + `Service.cs`
- **Models**: PascalCase entity names
- **Interfaces**: Start with `I` (e.g., `IGameService`)

### Separation of Concerns

#### **Components** (UI Layer)
- Handle user interaction
- Manage component state
- Call services for business logic
- No direct database access

#### **Services** (Business Logic)
- Implement business rules
- Coordinate between data and UI
- Handle complex operations
- Return domain objects

#### **Data Layer** (DbContext, Models)
- Define data structure
- Handle database operations
- Implement data validation
- Manage relationships

## 🔒 Security Best Practices

### Input Validation

```csharp
// Model validation
public class Game
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(450)]  // Limit length
    public string UserId { get; set; } = null!;
    
    [StringLength(1000)]  // Prevent oversized data
    public string GameState { get; set; } = string.Empty;
}

// Service-level validation
public async Task<Game> MakeMoveAsync(int gameId, string userId, int position)
{
    // Validate input range
    if (position < 0 || position >= 9)
        throw new ArgumentOutOfRangeException(nameof(position));
        
    // Validate ownership
    var game = await GetGameAsync(gameId, userId);
    if (game == null)
        throw new InvalidOperationException("Game not found or access denied");
}
```

### Cross-Site Scripting (XSS) Prevention

```html
<!-- Razor automatically HTML-encodes output -->
<h2>Welcome, @User.Identity.Name!</h2>  <!-- Safe -->

<!-- For raw HTML (use carefully) -->
<div>@Html.Raw(trustedHtmlContent)</div>  <!-- Only with trusted content -->
```

### SQL Injection Prevention

```csharp
// EF Core automatically parameterizes queries
var games = await _context.Games
    .Where(g => g.UserId == userId)  // Safe - parameterized
    .ToListAsync();

// Raw SQL (if needed) - still parameterized
var games = await _context.Games
    .FromSqlRaw("SELECT * FROM Games WHERE UserId = {0}", userId)
    .ToListAsync();
```

### CSRF Protection

```html
<!-- Automatic CSRF tokens in forms -->
<form method="post">
    <AntiforgeryToken />  <!-- Blazor adds this automatically -->
    <!-- Form content -->
</form>
```

## ⚡ Performance Optimization

### Database Performance

#### **Efficient Queries**
```csharp
// Good: Select only needed data
public async Task<UserStats> GetUserStatsAsync(string userId)
{
    return await _context.Users
        .Where(u => u.Id == userId)
        .Select(u => new UserStats 
        { 
            Wins = u.Wins, 
            TotalGames = u.TotalGamesPlayed 
        })
        .FirstOrDefaultAsync();
}

// Avoid: Loading full entities when not needed
```

#### **Connection Pooling**
```csharp
// Enabled by default in .NET Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
```

### Blazor Performance

#### **Component Optimization**
```csharp
// Use ShouldRender to control re-rendering
protected override bool ShouldRender()
{
    return hasStateChanged;
}

// Implement IDisposable for cleanup
public void Dispose()
{
    // Clean up resources, event handlers, etc.
}
```

#### **Efficient Event Handling**
```csharp
// Avoid creating new delegates in render
private async Task HandleClick() => await DoSomething();

// In markup
<button @onclick="HandleClick">Click</button>  // Good

// Avoid
<button @onclick="async () => await DoSomething()">Click</button>  // Creates new delegate each render
```

### Memory Management

```csharp
// Proper disposal of resources
public class GameService : IDisposable
{
    private readonly ApplicationDbContext _context;
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}

// Use 'using' statements for short-lived resources
public async Task<string> ProcessFile(string fileName)
{
    using var fileStream = File.OpenRead(fileName);
    // Process file
    return result;
}
```

## 🧪 Testing Strategies

### Unit Testing Setup

```csharp
// Test project structure
[TestClass]
public class GameServiceTests
{
    private ApplicationDbContext _context;
    private GameService _gameService;
    
    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
            
        _context = new ApplicationDbContext(options);
        _gameService = new GameService(_context);
    }
    
    [TestMethod]
    public async Task CreateNewGame_ShouldCreateGameWithEmptyBoard()
    {
        // Arrange
        var userId = "test-user";
        
        // Act
        var game = await _gameService.CreateNewGameAsync(userId);
        
        // Assert
        Assert.IsNotNull(game);
        Assert.AreEqual(userId, game.UserId);
        Assert.AreEqual(GameResult.InProgress, game.Result);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class GameIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public GameIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
    
    [TestMethod]
    public async Task HomePage_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/");
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

## 📏 Code Quality & Standards

### Code Analysis

```xml
<!-- Enable analyzers in project file -->
<PropertyGroup>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>CS1998</WarningsNotAsErrors>
    <CodeAnalysisRuleSet>ruleset.ruleset</CodeAnalysisRuleSet>
</PropertyGroup>

<PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" />
<PackageReference Include="StyleCop.Analyzers" Version="1.1.118" />
```

### Documentation Standards

```csharp
/// <summary>
/// Creates a new tic-tac-toe game for the specified user.
/// </summary>
/// <param name="userId">The unique identifier of the user creating the game.</param>
/// <returns>A task that represents the asynchronous operation. The task result contains the newly created game.</returns>
/// <exception cref="ArgumentNullException">Thrown when userId is null or empty.</exception>
public async Task<Game> CreateNewGameAsync(string userId)
{
    if (string.IsNullOrEmpty(userId))
        throw new ArgumentNullException(nameof(userId));
        
    // Implementation...
}
```

### Error Handling Patterns

```csharp
// Service layer error handling
public async Task<Game> MakeMoveAsync(int gameId, string userId, int position)
{
    try
    {
        // Validate input
        if (position < 0 || position >= 9)
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 0 and 8");
            
        // Business logic
        var game = await GetGameAsync(gameId, userId);
        if (game == null)
            throw new InvalidOperationException("Game not found or access denied");
            
        // Process move
        return ProcessMove(game, position);
    }
    catch (DbUpdateException ex)
    {
        // Log database errors
        _logger.LogError(ex, "Database error while making move for game {GameId}", gameId);
        throw new InvalidOperationException("Unable to save game state", ex);
    }
}

// Component error handling
private async Task MakeMove(int position)
{
    try
    {
        isProcessing = true;
        errorMessage = null;
        
        currentGame = await GameService.MakeMoveAsync(currentGame.Id, currentUser.Id, position);
        // Update UI state...
    }
    catch (Exception ex)
    {
        errorMessage = ex.Message;  // User-friendly error display
        _logger.LogError(ex, "Error making move at position {Position}", position);
    }
    finally
    {
        isProcessing = false;
    }
}
```

## 🎯 Key Takeaways

### Development Principles

1. **SOLID Principles**
   - **S**ingle Responsibility: Each class has one reason to change
   - **O**pen/Closed: Open for extension, closed for modification
   - **L**iskov Substitution: Derived classes must be substitutable
   - **I**nterface Segregation: Many specific interfaces over one general
   - **D**ependency Inversion: Depend on abstractions, not concretions

2. **DRY (Don't Repeat Yourself)**
   - Extract common functionality into services
   - Use inheritance and composition appropriately
   - Create reusable components

3. **KISS (Keep It Simple, Stupid)**
   - Prefer simple solutions over complex ones
   - Write self-documenting code
   - Avoid premature optimization

### .NET Ecosystem Best Practices

1. **Use latest .NET version** for best performance and features
2. **Follow async/await patterns** for I/O operations
3. **Implement proper error handling** at all layers
4. **Use dependency injection** for loose coupling
5. **Apply security-first thinking** from the start
6. **Write testable code** with clear separation of concerns
7. **Monitor performance** and optimize based on metrics

### Further Learning Resources

- **Official Documentation**: https://docs.microsoft.com/dotnet/
- **Blazor Documentation**: https://docs.microsoft.com/aspnet/core/blazor/
- **Entity Framework Core**: https://docs.microsoft.com/ef/core/
- **ASP.NET Core**: https://docs.microsoft.com/aspnet/core/
- **C# Programming Guide**: https://docs.microsoft.com/dotnet/csharp/
- **Design Patterns**: Gang of Four patterns in C#
- **Clean Architecture**: Robert C. Martin's Clean Architecture principles

---

This guide represents industry best practices and Microsoft's recommended approaches for building modern .NET applications. Continue learning and applying these patterns in your projects! 🚀