using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using HighLoad.HomeWork.SocialNetwork.CounterService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.CounterService.Models;
using HighLoad.HomeWork.SocialNetwork.CounterService.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Repositories
{
    public class CounterRepository : ICounterRepository
    {
        private readonly DatabaseOptions _dbOptions;
        private readonly ILogger<CounterRepository> _logger;

        public CounterRepository(IOptions<DatabaseOptions> dbOptions, ILogger<CounterRepository> logger)
        {
            _dbOptions = dbOptions.Value;
            _logger = logger;
        }

        private IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_dbOptions.ConnectionString);
        }

        public async Task<Counter?> GetCounterAsync(Guid userId, string type)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT id, user_id as UserId, type, count, last_updated as LastUpdated
                    FROM counters
                    WHERE user_id = @UserId AND type = @Type";

                return await connection.QueryFirstOrDefaultAsync<Counter>(sql, new { UserId = userId, Type = type });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении счетчика для пользователя {UserId} типа {Type}", userId, type);
                throw;
            }
        }

        public async Task<IEnumerable<Counter>> GetUserCountersAsync(Guid userId)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT id, user_id as UserId, type, count, last_updated as LastUpdated
                    FROM counters
                    WHERE user_id = @UserId";

                return await connection.QueryAsync<Counter>(sql, new { UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении счетчиков для пользователя {UserId}", userId);
                throw;
            }
        }

        public async Task<Counter> IncrementCounterAsync(Guid userId, string type, int amount = 1)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    INSERT INTO counters (user_id, type, count, last_updated)
                    VALUES (@UserId, @Type, @Amount, @Now)
                    ON CONFLICT (user_id, type)
                    DO UPDATE SET 
                        count = counters.count + @Amount,
                        last_updated = @Now
                    RETURNING id, user_id as UserId, type, count, last_updated as LastUpdated";

                return await connection.QueryFirstAsync<Counter>(sql, new 
                { 
                    UserId = userId, 
                    Type = type, 
                    Amount = amount, 
                    Now = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при инкрементировании счетчика для пользователя {UserId} типа {Type}", userId, type);
                throw;
            }
        }

        public async Task<Counter> DecrementCounterAsync(Guid userId, string type, int amount = 1)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    INSERT INTO counters (user_id, type, count, last_updated)
                    VALUES (@UserId, @Type, 0, @Now)
                    ON CONFLICT (user_id, type)
                    DO UPDATE SET 
                        count = GREATEST(0, counters.count - @Amount),
                        last_updated = @Now
                    RETURNING id, user_id as UserId, type, count, last_updated as LastUpdated";

                return await connection.QueryFirstAsync<Counter>(sql, new 
                { 
                    UserId = userId, 
                    Type = type, 
                    Amount = amount, 
                    Now = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при декрементировании счетчика для пользователя {UserId} типа {Type}", userId, type);
                throw;
            }
        }

        public async Task<Counter> ResetCounterAsync(Guid userId, string type)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    INSERT INTO counters (user_id, type, count, last_updated)
                    VALUES (@UserId, @Type, 0, @Now)
                    ON CONFLICT (user_id, type)
                    DO UPDATE SET 
                        count = 0,
                        last_updated = @Now
                    RETURNING id, user_id as UserId, type, count, last_updated as LastUpdated";

                return await connection.QueryFirstAsync<Counter>(sql, new 
                { 
                    UserId = userId, 
                    Type = type,
                    Now = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сбросе счетчика для пользователя {UserId} типа {Type}", userId, type);
                throw;
            }
        }

        public async Task<Counter> UpdateCounterAsync(Guid userId, string type, int newValue)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    INSERT INTO counters (user_id, type, count, last_updated)
                    VALUES (@UserId, @Type, @NewValue, @Now)
                    ON CONFLICT (user_id, type)
                    DO UPDATE SET 
                        count = @NewValue,
                        last_updated = @Now
                    RETURNING id, user_id as UserId, type, count, last_updated as LastUpdated";

                return await connection.QueryFirstAsync<Counter>(sql, new 
                { 
                    UserId = userId, 
                    Type = type, 
                    NewValue = Math.Max(0, newValue), 
                    Now = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении счетчика для пользователя {UserId} типа {Type}", userId, type);
                throw;
            }
        }
    }
} 