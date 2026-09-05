using System;

namespace SudokuEcosystem.Models
{
    public class PushNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public enum NotificationType
    {
        Info = 0,
        Promo = 1,
        Reminder = 2,
        Achievement = 3,
        System = 4
    }
}