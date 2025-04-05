using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.DialogService.Models;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Services;

internal sealed class DialogService : IDialogService
{
    private readonly string _citusConnectionString;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DialogService> _logger;

    public DialogService(
        string citusConnectionString, 
        IEventPublisher eventPublisher,
        ILogger<DialogService> logger)
    {
        _citusConnectionString = citusConnectionString;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task SaveMessageAsync(Guid receiverId, Guid senderId, string text)
    {
        const string sql = @"
                INSERT INTO messages (id, sender_id, receiver_id, text, created_at)
                VALUES (@p_id, @p_sender, @p_receiver, @p_text, @p_created)
            ";

        var msgId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        try
        {
            await using var conn = new NpgsqlConnection(_citusConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("p_id", msgId);
            cmd.Parameters.AddWithValue("p_sender", senderId);
            cmd.Parameters.AddWithValue("p_receiver", receiverId);
            cmd.Parameters.AddWithValue("p_text", text);
            cmd.Parameters.AddWithValue("p_created", now);
            await cmd.ExecuteNonQueryAsync();

            // Публикуем событие о новом сообщении
            await _eventPublisher.PublishNewMessageEventAsync(msgId, senderId, receiverId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении сообщения: RecipientId={RecipientId}, SenderId={SenderId}", 
                receiverId, senderId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<Message>> GetDialogAsync(Guid userId, Guid otherUserId)
    {
        const string sql = @"
                SELECT id, sender_id, receiver_id, text, created_at
                FROM messages
                WHERE 
                  (sender_id = @p_user AND receiver_id = @p_other)
                  OR
                  (sender_id = @p_other AND receiver_id = @p_user)
                ORDER BY created_at
            ";

        var result = new List<Message>();

        try
        {
            await using var conn = new NpgsqlConnection(_citusConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("p_user", userId);
            cmd.Parameters.AddWithValue("p_other", otherUserId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var msg = new Message
                {
                    Id = reader.GetGuid(0),
                    SenderId = reader.GetGuid(1),
                    ReceiverId = reader.GetGuid(2),
                    Text = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                };
                result.Add(msg);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении диалога: UserId={UserId}, OtherUserId={OtherUserId}", 
                userId, otherUserId);
            throw;
        }
    }
    
    public async Task MarkAsReadAsync(Guid userId, Guid messageId)
    {
        const string sql = @"
                UPDATE messages 
                SET is_read = true
                WHERE id = @p_id AND receiver_id = @p_user_id
            ";
        
        try
        {
            await using var conn = new NpgsqlConnection(_citusConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("p_id", messageId);
            cmd.Parameters.AddWithValue("p_user_id", userId);
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            
            // Если сообщение было обновлено (оно существует и принадлежит пользователю)
            if (rowsAffected > 0)
            {
                // Публикуем событие о прочтении сообщения
                await _eventPublisher.PublishMessageReadEventAsync(messageId, userId);
                _logger.LogInformation("Сообщение {MessageId} помечено как прочитанное пользователем {UserId}", 
                    messageId, userId);
            }
            else
            {
                _logger.LogWarning("Сообщение {MessageId} не найдено или не принадлежит пользователю {UserId}", 
                    messageId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при маркировке сообщения как прочитанного: MessageId={MessageId}, UserId={UserId}", 
                messageId, userId);
            throw;
        }
    }
    
    public async Task MarkManyAsReadAsync(Guid userId, IEnumerable<Guid> messageIds)
    {
        // Преобразуем список сообщений в список, чтобы не перечислять его несколько раз
        var messageIdsList = messageIds.ToList();
        if (!messageIdsList.Any())
        {
            return;
        }
        
        try
        {
            await using var conn = new NpgsqlConnection(_citusConnectionString);
            await conn.OpenAsync();
            
            // Формируем SQL запрос с параметрами для всех сообщений
            // В PostgreSQL можно использовать ANY для массива значений
            const string sql = @"
                UPDATE messages 
                SET is_read = true
                WHERE id = ANY(@p_ids) AND receiver_id = @p_user_id 
                RETURNING id
            ";
            
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("p_ids", messageIdsList.ToArray());
            cmd.Parameters.AddWithValue("p_user_id", userId);
            
            // Получаем список ID сообщений, которые были обновлены
            var updatedIds = new List<Guid>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                updatedIds.Add(reader.GetGuid(0));
            }
            
            // Публикуем события о прочтении сообщений
            foreach (var messageId in updatedIds)
            {
                await _eventPublisher.PublishMessageReadEventAsync(messageId, userId);
            }
            
            _logger.LogInformation(
                "Помечено как прочитанные {UpdatedCount} из {TotalCount} сообщений для пользователя {UserId}", 
                updatedIds.Count, messageIdsList.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при массовой маркировке сообщений как прочитанных: UserId={UserId}, MessageCount={MessageCount}", 
                userId, messageIdsList.Count);
            throw;
        }
    }
}