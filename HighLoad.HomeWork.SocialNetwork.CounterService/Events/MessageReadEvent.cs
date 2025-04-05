using System;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Events
{
    public class MessageReadEvent
    {
        public Guid UserId { get; set; }
        public Guid MessageId { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 