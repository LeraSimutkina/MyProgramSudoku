using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SudokuAPI.Data;

namespace SudokuAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("ban/{userId}")]
    public async Task<IActionResult> BanUser(int userId, [FromQuery] int days = 7)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("Пользователь не найден");

        user.IsBanned = true;
        user.BanExpiry = DateTime.Now.AddDays(days);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Пользователь {user.Login} забанен на {days} дней" });
    }

    [HttpPost("unban/{userId}")]
    public async Task<IActionResult> UnbanUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("Пользователь не найден");

        user.IsBanned = false;
        user.BanExpiry = null;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Пользователь {user.Login} разбанен" });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new { u.Id, u.Login, u.IsBanned, u.BanExpiry })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost("backup")]
    public async Task<IActionResult> BackupDatabase()
    {
        var backupPath = $"C:\\Users\\Пользователь\\source\\repos\\SudokuAPI\\Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

        var sql = $"BACKUP DATABASE [SudokuDB] TO DISK = '{backupPath}'";

        await _context.Database.ExecuteSqlRawAsync(sql);

        return Ok(new { message = $"Бэкап создан: {backupPath}" });
    }
    [HttpPost("restore")]
    public async Task<IActionResult> RestoreDatabase(string backupPath)
    {
        var sql = $"RESTORE DATABASE [SudokuDB] FROM DISK = '{backupPath}' WITH REPLACE";

        await _context.Database.ExecuteSqlRawAsync(sql);

        return Ok(new { message = $"БД восстановлена из: {backupPath}" });
    }
}