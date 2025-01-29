using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.DialogService.Models;
using StackExchange.Redis;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Services;

internal sealed class DialogServiceRedis(IConnectionMultiplexer redis) : IDialogService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task SaveMessageAsync(Guid receiverId, Guid senderId, string text)
    {
        var msgId = Guid.NewGuid();
        
        var createdAt = DateTime.UtcNow;
        var createdAtUnix = new DateTimeOffset(createdAt).ToUnixTimeSeconds();
        
        var messageKey = $"message:{msgId}";
        
        var dialogKey = GetDialogKey(receiverId, receiverId);
        
        var hashEntries = new[]
        {
            new HashEntry("id", msgId.ToString()),
            new HashEntry("senderId", senderId.ToString()),
            new HashEntry("receiverId", receiverId.ToString()),
            new HashEntry("text", text),
            new HashEntry("createdAt", createdAtUnix)
        };
        
        var batch = _db.CreateBatch();
        
        await batch.HashSetAsync(messageKey, hashEntries);
        
        await batch.SortedSetAddAsync(dialogKey, msgId.ToString(), createdAtUnix);
        
        batch.Execute();
    }

    public async Task<IReadOnlyCollection<Message>> GetDialogAsync(Guid userId, Guid otherUserId)
    {
        var dialogKey = GetDialogKey(userId, otherUserId);
        
        var messageIds = await _db.SortedSetRangeByRankAsync(dialogKey);

        if (messageIds.Length == 0)
            return Array.Empty<Message>();
        
        var batch = _db.CreateBatch();

        var tasks = new List<Task<HashEntry[]>>(messageIds.Length);

        foreach (var msgId in messageIds)
        {
            var key = $"message:{(string)msgId!}";
            var t = batch.HashGetAllAsync(key);
            tasks.Add(t);
        }

        batch.Execute();
        await Task.WhenAll(tasks);
        
        var result = new List<Message>(messageIds.Length);
        foreach (var t in tasks)
        {
            var entries = t.Result;
            if (entries.Length == 0) continue;

            var dict = entries.ToDictionary(x => x.Name.ToString(), x => x.Value);

            var id = Guid.Parse(dict["id"]!);
            var senderId = Guid.Parse(dict["senderId"]!);
            var receiverId = Guid.Parse(dict["receiverId"]!);
            var text = dict["text"].ToString();
            var createdAtUnix = (long)dict["createdAt"];
            var createdDt = DateTimeOffset.FromUnixTimeSeconds(createdAtUnix).UtcDateTime;

            result.Add(new Message
            {
                Id = id,
                SenderId = senderId,
                ReceiverId = receiverId,
                Text = text,
                CreatedAt = createdDt
            });
        }
        
        return result;
    }
    
    private static string GetDialogKey(Guid user1, Guid user2)
    {
        var s1 = user1.ToString();
        
        var s2 = user2.ToString();
        
        return string.CompareOrdinal(s1, s2) < 0 ? $"dialog:{s1}:{s2}" : $"dialog:{s2}:{s1}";
    }
}