using yawaflua.WebSockets.Attributes;
using yawaflua.WebSockets.Models.Abstracts;
using yawaflua.WebSockets.Models.Interfaces;

namespace Examples;

[WebSocket("/test")]
public class TestWebSocketServer : WebSocketController
{

    [WebSocket("/sub-test")]
    public override async Task OnMessageAsync(IWebSocket webSocket, HttpContext httpContext)
    {
        await webSocket.SendAsync("Test! Now on it endpoint: " + WebSocketManager.GetAllClients().Count(k => k.Path == webSocket.Client.Path));
    }
}