namespace SudokuAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "Admin" или "User"

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsBanned { get; set; }

        public DateTime? BanExpiry { get; set; }

        public int TotalGamesPlayed { get; set; }

        public int TotalWins { get; set; }

        public int TotalScore { get; set; }
    }
}