using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SudokuAPI.Data;
using SudokuAPI.Models;
using System.Security.Claims;

namespace SudokuAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProgressController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveProgress([FromBody] SaveProgressRequest request)
    {
        var userId = GetCurrentUserId();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound("Пользователь не найден");

        // ПРОВЕРКА: Ищем, есть ли уже у этого пользователя рекорд на ЭТОЙ сложности
        var existingProgress = await _context.GameProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Difficulty == request.Difficulty);

        if (existingProgress == null)
        {
            // Если записи нет — создаем новую
            var progress = new GameProgress
            {
                UserId = userId,
                Difficulty = request.Difficulty,
                Score = request.Score,
                TimeSeconds = request.TimeSeconds,
                CompletedAt = DateTime.Now
            };
            _context.GameProgresses.Add(progress);
        }
        else
        {
            // Если запись есть — обновляем её, только если новый результат ЛУЧШЕ (больше очков)
            if (request.Score > existingProgress.Score)
            {
                existingProgress.Score = request.Score;
                existingProgress.TimeSeconds = request.TimeSeconds;
                existingProgress.CompletedAt = DateTime.Now;
            }
        }

        // Обновляем общую статистику игрока (это оставляем для истории)
        user.TotalGamesPlayed++;
        user.TotalWins++;
        user.TotalScore += request.Score;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Прогресс обработан" });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = GetCurrentUserId();

        var stats = await _context.GameProgresses
            .Where(p => p.UserId == userId)
            .GroupBy(p => p.Difficulty)
            .Select(g => new
            {
                Difficulty = g.Key,
                GamesPlayed = g.Count(), // Теперь здесь всегда будет 1, так как мы храним только рекорды
                AverageScore = g.Average(p => p.Score),
                BestScore = g.Max(p => p.Score),
                AverageTime = g.Average(p => p.TimeSeconds)
            })
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        // Выводим топ-10 лучших рекордов. Дубликатов пользователей на одной сложности теперь не будет физически!
        var leaders = await _context.GameProgresses
            .Include(p => p.User)
            .OrderByDescending(p => p.Score)
            .Take(10)
            .Select(p => new
            {
                Login = p.User!.Login,
                Score = p.Score,
                Difficulty = p.Difficulty,
                CompletedAt = p.CompletedAt
            })
            .ToListAsync();

        return Ok(leaders);
    }
}

public class SaveProgressRequest
{
    public string Difficulty { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TimeSeconds { get; set; }
}