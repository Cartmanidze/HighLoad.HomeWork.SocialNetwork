using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HighLoad.HomeWork.SocialNetwork.CounterService.Models;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Interfaces
{
    public interface ICounterRepository
    {
        Task<Counter?> GetCounterAsync(Guid userId, string type);
        Task<IEnumerable<Counter>> GetUserCountersAsync(Guid userId);
        Task<Counter> IncrementCounterAsync(Guid userId, string type, int amount = 1);
        Task<Counter> DecrementCounterAsync(Guid userId, string type, int amount = 1);
        Task<Counter> ResetCounterAsync(Guid userId, string type);
        Task<Counter> UpdateCounterAsync(Guid userId, string type, int newValue);
    }
} 