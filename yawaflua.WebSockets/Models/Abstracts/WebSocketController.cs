using Microsoft.AspNetCore.Http;
using yawaflua.WebSockets.Core;
using yawaflua.WebSockets.Models.Interfaces;
using WebSocketManager = yawaflua.WebSockets.Core.WebSocketManager;

namespace yawaflua.WebSockets.Models.Abstracts;

public abstract  class WebSocketController : IWebSocketController
{
    /// <summary>
    /// WebsocketManager provides work with all clients
    /// </summary>
    public IWebSocketManager WebSocketManager => new WebSocketManager();
    
    /// <summary>
    /// Example of function OnMessage
    /// </summary>
    /// <param name="webSocket">WebSocket with provided data about user e.t.c.</param>
    /// <param name="httpContext">Http context of request</param>
    /// <returns></returns>
    public virtual Task OnMessageAsync(IWebSocket webSocket, HttpContext httpContext)
    {
        return Task.CompletedTask;
    }
}