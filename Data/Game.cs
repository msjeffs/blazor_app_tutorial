using System.ComponentModel.DataAnnotations;

namespace TicTacToeApp.Data;

public class Game
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = null!;
    
    public ApplicationUser User { get; set; } = null!;
    
    public GameResult Result { get; set; }
    
    public string GameState { get; set; } = string.Empty; // JSON representation of the board state
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    public bool IsCompleted { get; set; } = false;
}

public enum GameResult
{
    InProgress = 0,
    UserWin = 1,
    ComputerWin = 2,
    Draw = 3
}