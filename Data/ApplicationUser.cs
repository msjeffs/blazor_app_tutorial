using Microsoft.AspNetCore.Identity;

namespace TicTacToeApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public int Wins { get; set; } = 0;
    public int TotalGamesPlayed { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property for games
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}

