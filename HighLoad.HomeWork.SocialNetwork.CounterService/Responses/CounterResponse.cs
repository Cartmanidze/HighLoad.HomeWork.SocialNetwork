using System;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Responses
{
    public class CounterResponse
    {
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime LastUpdated { get; set; }
    }
} 