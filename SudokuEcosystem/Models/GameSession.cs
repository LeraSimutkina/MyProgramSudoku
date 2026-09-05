using System;

namespace SudokuEcosystem.Models
{
    public class GameSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Difficulty { get; set; }
        public int BoardSize { get; set; }
        public int Score { get; set; }
        public TimeSpan CompletionTime { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsWin { get; set; }
        public int HintsUsed { get; set; }
        public int MistakesMade { get; set; }
        public string MovesHistory { get; set; }
    }
}