# Tic Tac Toe Blazor App - Setup and Operation Tutorial

Welcome to the Tic Tac Toe Blazor application! This tutorial will guide you through setting up, running, and using the application.

## 📋 Table of Contents

- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Running the Application](#running-the-application)
- [Using the Application](#using-the-application)
- [Database Management](#database-management)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

## 🔧 Prerequisites

Before you begin, ensure you have the following installed on your system:

### Required Software
- **.NET 8 SDK** or later
  - Download from: https://dotnet.microsoft.com/download
  - Verify installation: `dotnet --version`

### Recommended Tools
- **Visual Studio 2022** (Community edition or higher) with ASP.NET workload
- **Visual Studio Code** with C# extension
- **Git** for version control

### System Requirements
- **Operating System**: Windows 10+, macOS 10.15+, or Linux
- **RAM**: 4GB minimum, 8GB recommended
- **Disk Space**: 2GB free space

## 🚀 Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/msjeffs/blazor_app_tutorial.git
cd blazor_app_tutorial
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Set Up the Database

The application uses SQLite with Entity Framework Core. Initialize the database:

```bash
# Install EF Core tools globally (if not already installed)
dotnet tool install --global dotnet-ef

# Apply database migrations
dotnet ef database update
```

This will create a SQLite database file at `Data/app.db` with all necessary tables.

### 4. Build the Application

```bash
dotnet build
```

## 🏃‍♂️ Running the Application

### Development Mode

```bash
dotnet run
```

The application will start and be available at:
- **HTTP**: http://localhost:5113
- **HTTPS**: https://localhost:7113 (if SSL is configured)

### Production Mode

```bash
dotnet run --environment Production
```

### Watch Mode (Auto-reload during development)

```bash
dotnet watch run
```

## 🎮 Using the Application

### 1. Home Page
- Navigate to the home page to see the application overview
- Features are displayed with descriptions
- Authentication status is shown with appropriate action buttons

### 2. User Registration
1. Click **"Sign Up"** on the home page or navigation
2. Fill in the registration form:
   - **Email**: Your email address (used as username)
   - **Password**: Must meet security requirements
   - **Confirm Password**: Must match the password
3. Click **"Register"** to create your account
4. You'll be automatically logged in after successful registration

### 3. User Login
1. Click **"Log In"** from the home page or navigation
2. Enter your credentials:
   - **Email**: The email you registered with
   - **Password**: Your password
3. Click **"Log in"** to access the application

### 4. Playing Tic Tac Toe

#### Starting a Game
1. After logging in, click **"Play Game"** in the navigation or **"Start Playing"** on the home page
2. A new game will automatically start
3. You play as **X** (blue), and the computer plays as **O** (teal)

#### Game Rules
- Click on any empty square to make your move
- The computer will automatically make its move after yours
- Win by getting three of your symbols in a row (horizontal, vertical, or diagonal)
- The game ends when someone wins or all squares are filled (draw)

#### Game Features
- **Win Tracking**: Your statistics are displayed at the top of the game
  - **Wins**: Number of games you've won
  - **Games**: Total games played
  - **Win Rate**: Percentage of games won
- **Smart AI**: The computer uses intelligent logic to:
  - Try to win when possible
  - Block your winning moves
  - Make strategic moves
- **Responsive Design**: Works on mobile phones, tablets, and desktops

#### After a Game
- View the result message (win, lose, or draw)
- Click **"Play Again"** to start a new game
- Your statistics update automatically

### 5. User Account Management
1. Click on your **username** in the navigation
2. Access account management features:
   - Change password
   - Update email
   - Delete account
   - Download personal data

### 6. Logging Out
- Click **"Logout"** in the navigation to sign out
- You'll be redirected to the home page

## 🗄️ Database Management

### Database Location
- **Development**: `Data/app.db` (SQLite file)
- **File Size**: Small (typically under 1MB for normal usage)

### Adding New Migrations
If you modify the data models:

```bash
# Create a new migration
dotnet ef migrations add YourMigrationName

# Apply the migration
dotnet ef database update
```

### Database Reset
To reset the database (WARNING: This deletes all data):

```bash
# Remove the database file
rm Data/app.db

# Recreate the database
dotnet ef database update
```

### Viewing Database Contents
You can use SQLite tools to view the database:

```bash
# Using sqlite3 command line (if installed)
sqlite3 Data/app.db
.tables
.schema
SELECT * FROM AspNetUsers;
SELECT * FROM Games;
```

## 🌐 Deployment

### Prerequisites for Deployment
- Web server supporting .NET 8 (IIS, Apache, Nginx)
- HTTPS certificate (recommended)
- SQL Server or keep SQLite for small deployments

### Local Production Build

```bash
# Build for production
dotnet publish -c Release -o ./publish

# Run the published application
cd publish
dotnet TicTacToeApp.dll
```

### Configuration for Production

1. **Update Connection String** (for SQL Server):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TicTacToeApp;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

2. **Environment Variables**:
   ```bash
   export ASPNETCORE_ENVIRONMENT=Production
   export ASPNETCORE_URLS=http://localhost:5000
   ```

3. **HTTPS Configuration**:
   - Configure SSL certificates
   - Update `appsettings.Production.json` with HTTPS settings

### Cloud Deployment Options
- **Azure App Service**: Deploy directly from Visual Studio
- **AWS Elastic Beanstalk**: Deploy .NET applications
- **Docker**: Containerize the application
- **GitHub Pages**: For static Blazor WebAssembly (requires modification)

## 🔧 Troubleshooting

### Common Issues

#### 1. Database Connection Errors
**Problem**: "Database connection failed" or migration errors

**Solutions**:
```bash
# Check if database file exists
ls -la Data/

# Reset database
rm Data/app.db
dotnet ef database update

# Check EF Core tools
dotnet ef --version
```

#### 2. Build Errors
**Problem**: Compilation errors or missing packages

**Solutions**:
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Check .NET version
dotnet --version
```

#### 3. Port Already in Use
**Problem**: "Port 5113 is already in use"

**Solutions**:
```bash
# Use different port
dotnet run --urls "http://localhost:5114"

# Kill process using the port (Linux/Mac)
lsof -ti:5113 | xargs kill -9

# Kill process using the port (Windows)
netstat -ano | findstr :5113
taskkill /PID <ProcessID> /F
```

#### 4. Authentication Issues
**Problem**: Login redirects or authentication not working

**Solutions**:
- Clear browser cookies
- Check if Identity tables exist in database
- Verify `appsettings.json` configuration
- Reset user data if needed

#### 5. CSS/Styling Issues
**Problem**: Styles not loading or appearing incorrectly

**Solutions**:
```bash
# Hard refresh browser (Ctrl+F5)
# Clear browser cache
# Check if CSS files exist in wwwroot
# Rebuild the application
dotnet clean && dotnet build
```

### Performance Issues

#### Slow Game Loading
- Check database connection
- Verify Entity Framework queries
- Monitor network requests in browser dev tools

#### High Memory Usage
- Restart the application
- Check for memory leaks in browser dev tools
- Monitor with dotnet diagnostic tools

### Getting Help

1. **Check Application Logs**:
   - Console output during development
   - Event logs in production

2. **Browser Developer Tools**:
   - Console tab for JavaScript errors
   - Network tab for failed requests
   - Application tab for local storage issues

3. **Database Issues**:
   - Use SQLite browser tools
   - Check Entity Framework logs
   - Verify migration status

## 📊 Application Architecture

### Technology Stack
- **Frontend**: Blazor Server (real-time UI)
- **Backend**: ASP.NET Core 8
- **Database**: SQLite with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Styling**: Custom CSS with responsive design

### Key Components
- **GameService**: Handles game logic and AI
- **ApplicationDbContext**: Database context
- **Identity Pages**: User authentication
- **Razor Components**: UI components

### Security Features
- Password hashing with Identity
- CSRF protection
- SQL injection prevention (EF Core)
- XSS protection (Razor encoding)

## 🎯 Tips for Success

1. **Regular Backups**: Backup your database file regularly
2. **Version Control**: Use Git to track changes
3. **Testing**: Test on different devices and browsers
4. **Monitoring**: Monitor application performance
5. **Updates**: Keep .NET and packages updated

## 📝 Support

For additional help:
- Review the `learn.md` file for technical details
- Check the GitHub repository for issues
- Refer to official .NET and Blazor documentation
- ASP.NET Core documentation: https://docs.microsoft.com/aspnet/core

---

**Happy Gaming! 🎮**