using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SudokuAPI.Data;
using SudokuAPI.Models;
using System.Security.Claims;

namespace SudokuAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? int.Parse(userIdClaim) : 0;
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetCurrentUserId();
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    [Authorize]
    [HttpPost("read/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Уведомление прочитано" });
    }

    [HttpPost("send-discount")]
    public async Task<IActionResult> SendDiscountNotification(int userId, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "🎉 Скидка!",
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Уведомление отправлено" });
    }

    [HttpPost("send-overdue")]
    public async Task<IActionResult> SendOverdueNotification(int userId, string taskName)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "⚠️ Просрок!",
            Message = $"Задание '{taskName}' просрочено!",
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Уведомление о просроке отправлено" });
    }
}