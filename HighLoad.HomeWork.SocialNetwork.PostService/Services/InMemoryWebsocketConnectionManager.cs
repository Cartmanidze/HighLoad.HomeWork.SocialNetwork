using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Services;

internal sealed class InMemoryWebsocketConnectionManager : IWebsocketConnectionManager
{
    private readonly ConcurrentDictionary<Guid, HashSet<WebSocket>> _connections 
        = new();
    public Task AddConnectionAsync(Guid userId, WebSocket socket)
    {
        var set = _connections.GetOrAdd(userId, _ => []);
        
        lock (set)
        {
            set.Add(socket);
        }

        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(Guid userId, WebSocket socket)
    {
        if (_connections.TryGetValue(userId, out var set))
        {
            lock (set)
            {
                set.Remove(socket);
                
                if (set.Count == 0)
                {
                    _connections.TryRemove(userId, out _);
                }
            }
        }

        return Task.CompletedTask;
    }

    public async Task SendToUserAsync(Guid userId, string message)
    {
        if (!_connections.TryGetValue(userId, out var set))
            return;

        var bytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(bytes);
        
        List<WebSocket> socketsSnapshot;
        lock (set)
        {
            socketsSnapshot = set.ToList();
        }

        foreach (var ws in socketsSnapshot)
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(segment, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
            }
        }
    }

    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string message)
    {
        var distinctUserIds = userIds.Distinct().ToList();

        var bytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(bytes);

        foreach (var uid in distinctUserIds)
        {
            if (_connections.TryGetValue(uid, out var set))
            {
                List<WebSocket> socketsSnapshot;
                lock (set)
                {
                    socketsSnapshot = set.ToList();
                }

                foreach (var ws in socketsSnapshot)
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        await ws.SendAsync(segment, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
                    }
                }
            }
        }
    }
}