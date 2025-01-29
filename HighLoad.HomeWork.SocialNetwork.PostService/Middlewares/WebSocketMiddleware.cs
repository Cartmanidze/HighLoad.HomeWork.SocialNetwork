using System.Net.WebSockets;
using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Middlewares;

public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;

    public WebSocketMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IWebsocketConnectionManager wsManager)
    {
        if (context.Request.Path == "/post/feed/posted" && context.WebSockets.IsWebSocketRequest)
        {
            
            var userIdStr = context.User?.FindFirst("UserId")?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                context.Response.StatusCode = 401;
                return;
            }
            
            var ws = await context.WebSockets.AcceptWebSocketAsync();

            await wsManager.AddConnectionAsync(userId, ws);
            await WebSocketLoopAsync(ws, userId, wsManager);
        }
        else
        {
            await _next(context);
        }
    }

    private async Task WebSocketLoopAsync(WebSocket socket, Guid userId, IWebsocketConnectionManager manager)
    {
        var buffer = new byte[4096];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }
        }
        await manager.RemoveConnectionAsync(userId, socket);
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
    }
}