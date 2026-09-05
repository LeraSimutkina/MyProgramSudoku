using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SudokuAPI.Data;
using SudokuAPI.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SudokuAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        
        if (await _context.Users.AnyAsync(u => u.Login == request.Login))
        {
            return BadRequest("Пользователь с таким логином уже существует.");
        }

        var user = new User
        {
            Login = request.Login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Регистрация успешна."
        });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);

        if (user == null)
            return Unauthorized("Неверный логин или пароль");

        
        if (user.IsBanned)
        {
            if (user.BanExpiry.HasValue && user.BanExpiry.Value > DateTime.UtcNow)
            {
                return StatusCode(403, $"Доступ заблокирован. Аккаунт временно заморожен до: {user.BanExpiry.Value.ToLocalTime():dd.MM.yyyy HH:mm:ss}");
            }
            else
            {
                user.IsBanned = false;
                user.BanExpiry = null;
                await _context.SaveChangesAsync();
            }
        }

        
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.IsBanned = true;
            user.BanExpiry = DateTime.UtcNow.AddMinutes(5); 
            await _context.SaveChangesAsync();

            return Unauthorized("Неверный логин или пароль. В целях безопасности аккаунт заблокирован на 5 минут.");
        }

        var token = GenerateJwtToken(user);

        
        return Ok(new { token, userId = user.Id, role = user.Role });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["Jwt:Secret"];

        if (string.IsNullOrWhiteSpace(jwtSecret))
            throw new InvalidOperationException("JWT secret is not configured.");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Login),
        new Claim(ClaimTypes.Role, user.Role)
    };

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds,
            claims: claims
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


public class RegisterRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
   
}

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}