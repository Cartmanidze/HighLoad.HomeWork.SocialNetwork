using System;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Requests
{
    public class CounterUpdateRequest
    {
        public string Type { get; set; } = "UnreadMessages";
        public int Value { get; set; }
    }
} 