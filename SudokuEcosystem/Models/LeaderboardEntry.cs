using System;

namespace SudokuEcosystem.Models
{
    public class LeaderboardEntry
    {
        public string Login { get; set; }

        public int Score { get; set; }

        public string Difficulty { get; set; }

        public DateTime CompletedAt { get; set; }
    }
}