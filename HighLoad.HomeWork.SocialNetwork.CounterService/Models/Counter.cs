using System;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Models
{
    public class Counter
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = "UnreadMessages"; // Тип счетчика (например, непрочитанные сообщения)
        public int Count { get; set; }
        public DateTime LastUpdated { get; set; }
    }
} 