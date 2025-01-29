using System.Net.WebSockets;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

public interface IWebsocketConnectionManager
{
    Task AddConnectionAsync(Guid userId, WebSocket socket);
    
    Task RemoveConnectionAsync(Guid userId, WebSocket socket);
    
    Task SendToUserAsync(Guid userId, string message);
    
    Task SendToUsersAsync(IEnumerable<Guid> userIds, string message);
}