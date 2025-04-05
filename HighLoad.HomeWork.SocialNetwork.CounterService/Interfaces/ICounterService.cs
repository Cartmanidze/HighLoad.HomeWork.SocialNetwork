using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HighLoad.HomeWork.SocialNetwork.CounterService.Models;
using HighLoad.HomeWork.SocialNetwork.CounterService.Responses;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Interfaces
{
    public interface ICounterService
    {
        Task<CounterResponse?> GetCounterAsync(Guid userId, string type = "UnreadMessages");
        Task<IEnumerable<CounterResponse>> GetUserCountersAsync(Guid userId);
        Task<CounterResponse> IncrementCounterAsync(Guid userId, string type = "UnreadMessages", int amount = 1);
        Task<CounterResponse> DecrementCounterAsync(Guid userId, string type = "UnreadMessages", int amount = 1);
        Task<CounterResponse> ResetCounterAsync(Guid userId, string type = "UnreadMessages");
        Task<CounterResponse> UpdateCounterAsync(Guid userId, int newValue, string type = "UnreadMessages");
    }
} 