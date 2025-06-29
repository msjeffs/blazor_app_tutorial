using System.Text.Json;
using TicTacToeApp.Data;
using Microsoft.EntityFrameworkCore;

namespace TicTacToeApp.Services;

public class GameService
{
    private readonly ApplicationDbContext _context;
    
    public GameService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Game> CreateNewGameAsync(string userId)
    {
        var game = new Game
        {
            UserId = userId,
            GameState = JsonSerializer.Serialize(new string[9]), // Empty 3x3 board
            Result = GameResult.InProgress
        };
        
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        
        return game;
    }
    
    public async Task<Game?> GetGameAsync(int gameId, string userId)
    {
        return await _context.Games
            .FirstOrDefaultAsync(g => g.Id == gameId && g.UserId == userId);
    }
    
    public async Task<Game> MakeMoveAsync(int gameId, string userId, int position)
    {
        var game = await GetGameAsync(gameId, userId);
        if (game == null || game.IsCompleted)
            throw new InvalidOperationException("Game not found or already completed");
            
        var board = JsonSerializer.Deserialize<string[]>(game.GameState) ?? new string[9];
        
        if (!string.IsNullOrEmpty(board[position]))
            throw new InvalidOperationException("Position already occupied");
            
        // Player move
        board[position] = "X";
        
        // Check if player won
        if (CheckWin(board, "X"))
        {
            game.Result = GameResult.UserWin;
            game.IsCompleted = true;
            game.CompletedAt = DateTime.UtcNow;
            
            // Update user wins
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Wins++;
                user.TotalGamesPlayed++;
            }
        }
        else if (IsBoardFull(board))
        {
            game.Result = GameResult.Draw;
            game.IsCompleted = true;
            game.CompletedAt = DateTime.UtcNow;
            
            // Update user total games
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.TotalGamesPlayed++;
            }
        }
        else
        {
            // Computer move (simple AI)
            var computerMove = GetBestComputerMove(board);
            if (computerMove != -1)
            {
                board[computerMove] = "O";
                
                if (CheckWin(board, "O"))
                {
                    game.Result = GameResult.ComputerWin;
                    game.IsCompleted = true;
                    game.CompletedAt = DateTime.UtcNow;
                    
                    // Update user total games
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.TotalGamesPlayed++;
                    }
                }
                else if (IsBoardFull(board))
                {
                    game.Result = GameResult.Draw;
                    game.IsCompleted = true;
                    game.CompletedAt = DateTime.UtcNow;
                    
                    // Update user total games
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.TotalGamesPlayed++;
                    }
                }
            }
        }
        
        game.GameState = JsonSerializer.Serialize(board);
        await _context.SaveChangesAsync();
        
        return game;
    }
    
    public async Task<List<Game>> GetUserGamesAsync(string userId, int pageSize = 10)
    {
        return await _context.Games
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .Take(pageSize)
            .ToListAsync();
    }
    
    private bool CheckWin(string[] board, string player)
    {
        // Check rows, columns, and diagonals
        int[][] winConditions = {
            [0, 1, 2], [3, 4, 5], [6, 7, 8], // rows
            [0, 3, 6], [1, 4, 7], [2, 5, 8], // columns
            [0, 4, 8], [2, 4, 6]             // diagonals
        };
        
        return winConditions.Any(condition => 
            condition.All(pos => board[pos] == player));
    }
    
    private bool IsBoardFull(string[] board)
    {
        return board.All(cell => !string.IsNullOrEmpty(cell));
    }
    
    private int GetBestComputerMove(string[] board)
    {
        // Simple AI: try to win, then block player, then take center or corner
        
        // Try to win
        for (int i = 0; i < 9; i++)
        {
            if (string.IsNullOrEmpty(board[i]))
            {
                board[i] = "O";
                if (CheckWin(board, "O"))
                {
                    board[i] = string.Empty;
                    return i;
                }
                board[i] = string.Empty;
            }
        }
        
        // Try to block player
        for (int i = 0; i < 9; i++)
        {
            if (string.IsNullOrEmpty(board[i]))
            {
                board[i] = "X";
                if (CheckWin(board, "X"))
                {
                    board[i] = string.Empty;
                    return i;
                }
                board[i] = string.Empty;
            }
        }
        
        // Take center if available
        if (string.IsNullOrEmpty(board[4]))
            return 4;
            
        // Take corners
        int[] corners = { 0, 2, 6, 8 };
        var availableCorner = corners.FirstOrDefault(c => string.IsNullOrEmpty(board[c]));
        if (availableCorner != 0 || string.IsNullOrEmpty(board[0]))
            return availableCorner;
            
        // Take any available space
        for (int i = 0; i < 9; i++)
        {
            if (string.IsNullOrEmpty(board[i]))
                return i;
        }
        
        return -1; // No moves available
    }
}