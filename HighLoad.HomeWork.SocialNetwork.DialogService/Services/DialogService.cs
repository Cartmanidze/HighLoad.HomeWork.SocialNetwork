using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.DialogService.Models;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Services;

internal sealed class DialogService(string citusConnectionString) : IDialogService
{
    public async Task SaveMessageAsync(Guid senderId, Guid receiverId, string text)
    {
        const string sql = @"
                INSERT INTO messages (id, sender_id, receiver_id, text, created_at)
                VALUES (@p_id, @p_sender, @p_receiver, @p_text, @p_created)
            ";

        var msgId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var conn = new NpgsqlConnection(citusConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_id", msgId);
        cmd.Parameters.AddWithValue("p_sender", senderId);
        cmd.Parameters.AddWithValue("p_receiver", receiverId);
        cmd.Parameters.AddWithValue("p_text", text);
        cmd.Parameters.AddWithValue("p_created", now);
        await cmd.ExecuteNonQueryAsync();
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

        await using var conn = new NpgsqlConnection(citusConnectionString);
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
}