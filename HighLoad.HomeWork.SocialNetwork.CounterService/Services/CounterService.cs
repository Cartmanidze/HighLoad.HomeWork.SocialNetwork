using System.Text.Json;
using HighLoad.HomeWork.SocialNetwork.CounterService.Events;
using HighLoad.HomeWork.SocialNetwork.CounterService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.CounterService.Models;
using HighLoad.HomeWork.SocialNetwork.CounterService.Options;
using HighLoad.HomeWork.SocialNetwork.CounterService.Responses;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Services;

public class CounterService(
    ICounterRepository counterRepository,
    IDistributedCache cache,
    IPublishEndpoint publishEndpoint,
    IOptions<RedisOptions> redisOptions,
    ILogger<CounterService> logger)
    : ICounterService
{
    private readonly RedisOptions _redisOptions = redisOptions.Value;

    private string GetCounterCacheKey(Guid userId, string type) => $"counter:{userId}:{type}";
    private string GetUserCountersCacheKey(Guid userId) => $"counters:{userId}";

    public async Task<CounterResponse?> GetCounterAsync(Guid userId, string type = "UnreadMessages")
    {
        try
        {
            // Сначала проверяем кэш
            var cacheKey = GetCounterCacheKey(userId, type);
            var cachedCounter = await cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedCounter))
            {
                logger.LogInformation("Счетчик для пользователя {UserId} типа {Type} получен из кэша", userId, type);
                return JsonSerializer.Deserialize<CounterResponse>(cachedCounter);
            }

            // Если в кэше нет, обращаемся к репозиторию
            var counter = await counterRepository.GetCounterAsync(userId, type);
            if (counter == null)
            {
                return null;
            }

            // Преобразуем в ответ и кэшируем результат
            var response = MapToResponse(counter);
            await CacheCounterAsync(response);
                
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении счетчика для пользователя {UserId} типа {Type}", userId, type);
            throw;
        }
    }

    public async Task<IEnumerable<CounterResponse>> GetUserCountersAsync(Guid userId)
    {
        try
        {
            // Сначала проверяем кэш
            var cacheKey = GetUserCountersCacheKey(userId);
            var cachedCounters = await cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedCounters))
            {
                logger.LogInformation("Счетчики для пользователя {UserId} получены из кэша", userId);
                return JsonSerializer.Deserialize<IEnumerable<CounterResponse>>(cachedCounters) ?? Enumerable.Empty<CounterResponse>();
            }

            // Если в кэше нет, обращаемся к репозиторию
            var counters = await counterRepository.GetUserCountersAsync(userId);
            var response = counters.Select(MapToResponse).ToList();

            // Кэшируем результат
            await cache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_redisOptions.DefaultExpirationMinutes)
                });

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении счетчиков для пользователя {UserId}", userId);
            throw;
        }
    }

    public async Task<CounterResponse> IncrementCounterAsync(Guid userId, string type = "UnreadMessages", int amount = 1)
    {
        try
        {
            var counter = await counterRepository.IncrementCounterAsync(userId, type, amount);
            var response = MapToResponse(counter);

            // Обновляем кэш
            await CacheCounterAsync(response);
            // Удаляем список всех счетчиков пользователя из кэша
            await cache.RemoveAsync(GetUserCountersCacheKey(userId));

            // Публикуем событие об обновлении счетчика
            await PublishCounterUpdatedEventAsync(counter);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инкрементировании счетчика для пользователя {UserId} типа {Type}", userId, type);
            throw;
        }
    }

    public async Task<CounterResponse> DecrementCounterAsync(Guid userId, string type = "UnreadMessages", int amount = 1)
    {
        try
        {
            var counter = await counterRepository.DecrementCounterAsync(userId, type, amount);
            var response = MapToResponse(counter);

            // Обновляем кэш
            await CacheCounterAsync(response);
            // Удаляем список всех счетчиков пользователя из кэша
            await cache.RemoveAsync(GetUserCountersCacheKey(userId));

            // Публикуем событие об обновлении счетчика
            await PublishCounterUpdatedEventAsync(counter);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при декрементировании счетчика для пользователя {UserId} типа {Type}", userId, type);
            throw;
        }
    }

    public async Task<CounterResponse> ResetCounterAsync(Guid userId, string type = "UnreadMessages")
    {
        try
        {
            var counter = await counterRepository.ResetCounterAsync(userId, type);
            var response = MapToResponse(counter);

            // Обновляем кэш
            await CacheCounterAsync(response);
            // Удаляем список всех счетчиков пользователя из кэша
            await cache.RemoveAsync(GetUserCountersCacheKey(userId));

            // Публикуем событие об обновлении счетчика
            await PublishCounterUpdatedEventAsync(counter);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при сбросе счетчика для пользователя {UserId} типа {Type}", userId, type);
            throw;
        }
    }

    public async Task<CounterResponse> UpdateCounterAsync(Guid userId, int newValue, string type = "UnreadMessages")
    {
        try
        {
            var counter = await counterRepository.UpdateCounterAsync(userId, type, newValue);
            var response = MapToResponse(counter);

            // Обновляем кэш
            await CacheCounterAsync(response);
            // Удаляем список всех счетчиков пользователя из кэша
            await cache.RemoveAsync(GetUserCountersCacheKey(userId));

            // Публикуем событие об обновлении счетчика
            await PublishCounterUpdatedEventAsync(counter);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении счетчика для пользователя {UserId} типа {Type}", userId, type);
            throw;
        }
    }

    private async Task CacheCounterAsync(CounterResponse counter)
    {
        var cacheKey = GetCounterCacheKey(counter.UserId, counter.Type);
        await cache.SetStringAsync(
            cacheKey, 
            JsonSerializer.Serialize(counter),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_redisOptions.DefaultExpirationMinutes)
            });
    }

    private async Task PublishCounterUpdatedEventAsync(Counter counter)
    {
        try
        {
            await publishEndpoint.Publish<CounterUpdatedEvent>(new
            {
                UserId = counter.UserId,
                Type = counter.Type,
                Count = counter.Count,
                Timestamp = DateTime.UtcNow
            });

            logger.LogInformation("Опубликовано событие обновления счетчика для пользователя {UserId} типа {Type}", counter.UserId, counter.Type);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при публикации события обновления счетчика для пользователя {UserId} типа {Type}", counter.UserId, counter.Type);
        }
    }

    private CounterResponse MapToResponse(Counter counter)
    {
        return new CounterResponse
        {
            UserId = counter.UserId,
            Type = counter.Type,
            Count = counter.Count,
            LastUpdated = counter.LastUpdated
        };
    }
}