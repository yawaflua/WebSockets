using yawaflua.WebSockets.Attributes;
using yawaflua.WebSockets.Models.Abstracts;
using WebSocket = yawaflua.WebSockets.Core.WebSocket;

namespace Examples;

[WebSocket("/chat")]
public class ChatController : WebSocketController
{
    
    public override async Task OnMessageAsync(
        WebSocket webSocket, 
        HttpContext httpContext)
    {
        await WebSocketManager.Broadcast(k => k.Path == "/chat", $"{webSocket.Client.Id}: {webSocket.Message}");
    }
}