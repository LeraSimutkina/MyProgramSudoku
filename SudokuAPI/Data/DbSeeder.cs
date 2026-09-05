using Microsoft.EntityFrameworkCore;
using SudokuAPI.Models;

namespace SudokuAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
    AppDbContext context,
    IConfiguration configuration)
        {
            if (await context.Users.AnyAsync(u => u.Role == "Admin"))
            {
                return;
            }

            var adminLogin = configuration["Admin:Login"];
            var adminPassword = configuration["Admin:Password"];

            if (string.IsNullOrWhiteSpace(adminLogin) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            if (await context.Users.AnyAsync(u => u.Login == adminLogin))
            {
                return;
            }

            var admin = new User
            {
                Login = adminLogin,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = "Admin"
            };

            context.Users.Add(admin);

            await context.SaveChangesAsync();
        }
    }
}