using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using yawaflua.WebSockets.Attributes;
using yawaflua.WebSockets.Core;
using yawaflua.WebSockets.Models.Abstracts;

namespace Examples;

[WebSocket("/test")]
public class TestWebSocketServer : WebSocketController
{

    [WebSocket("/sub-test")]
    public override async Task OnMessageAsync(WebSocket webSocket, HttpContext httpContext)
    {
        await webSocket.SendAsync("Test! Now on it endpoint: " + WebSocketManager.GetAllClients().Count(k => k.Path == webSocket.Client.Path));
    }
}