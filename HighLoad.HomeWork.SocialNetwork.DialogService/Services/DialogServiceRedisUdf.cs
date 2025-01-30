using System.Text.Json;
using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.DialogService.Models;
using StackExchange.Redis;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Services;

internal sealed class DialogServiceRedisUdf(IConnectionMultiplexer redis) : IDialogService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task SaveMessageAsync(Guid receiverId, Guid senderId, string text)
    {
        // Генерируем msgId на .NET стороне
        var msgId = Guid.NewGuid().ToString();

        // Вызываем Redis Function "send_message"
        //   FCALL send_message 0 <msgId> <senderId> <receiverId> <text>
        //  "0" = кол-во ключей (keys), так как мы всё передаём в args
        var result = await _db.ExecuteAsync("FCALL", 
            "send_message", 
            0, 
            msgId,
            senderId.ToString(),
            receiverId.ToString(),
            text
        );

        Console.WriteLine(result);
    }

    public async Task<IReadOnlyCollection<Message>> GetDialogAsync(Guid userId, Guid otherUserId)
    {
        // Вызываем Redis Function "get_dialog"
        var redisResult = await _db.ExecuteAsync("FCALL",
            "get_dialog",
            0,
            userId.ToString(),
            otherUserId.ToString()
        );

        // redisResult должен содержать JSON-строку (массива сообщений)
        var json = (string)redisResult!;
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<Message>();

        // Парсим JSON
        var list = JsonSerializer.Deserialize<List<MessageDto>>(json);
        if (list == null || list.Count == 0)
            return Array.Empty<Message>();

        // Преобразуем в нашу модель Message
        var result = new List<Message>(list.Count);
        foreach (var dto in list)
        {
            var createdAtUnix = long.Parse(dto.createdAt);
            var createdDt = DateTimeOffset.FromUnixTimeSeconds(createdAtUnix).UtcDateTime;

            result.Add(new Message
            {
                Id         = Guid.Parse(dto.id),
                SenderId   = Guid.Parse(dto.senderId),
                ReceiverId = Guid.Parse(dto.receiverId),
                Text       = dto.text,
                CreatedAt  = createdDt
            });
        }

        return result;
    }

    // вспомогательный класс для десериализации JSON
    private class MessageDto
    {
        public required string id { get; set; }
        public required string senderId { get; set; }
        public required string receiverId { get; set; }
        public required string text { get; set; }
        public required string createdAt { get; set; }
    }
}
