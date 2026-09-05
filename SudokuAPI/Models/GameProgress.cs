namespace SudokuAPI.Models
{
    public class GameProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TimeSeconds { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}