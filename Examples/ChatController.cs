using yawaflua.WebSockets.Attributes;
using yawaflua.WebSockets.Models.Abstracts;
using yawaflua.WebSockets.Models.Interfaces;

namespace Examples;

[WebSocket("/chat")]
public class ChatController : WebSocketController
{
    
    public override async Task OnMessageAsync(
        IWebSocket webSocket, 
        HttpContext httpContext)
    {
        await WebSocketManager.Broadcast(k => k.Path == "/chat", $"{webSocket.Client.Id}: {webSocket.Message}");
    }
}