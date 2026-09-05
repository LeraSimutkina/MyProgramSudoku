using Microsoft.EntityFrameworkCore;
using SudokuAPI.Models;

namespace SudokuAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<GameProgress> GameProgresses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}