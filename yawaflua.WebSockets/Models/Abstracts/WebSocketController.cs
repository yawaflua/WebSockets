using Microsoft.AspNetCore.Http;
using yawaflua.WebSockets.Core;
using yawaflua.WebSockets.Models.Interfaces;
using WebSocketManager = yawaflua.WebSockets.Core.WebSocketManager;

namespace yawaflua.WebSockets.Models.Abstracts;

public abstract  class WebSocketController : IWebSocketController
{
    public IWebSocketManager WebSocketManager => new WebSocketManager();
    
    public virtual Task OnMessageAsync(WebSocket webSocket, HttpContext httpContext)
    {
        return Task.CompletedTask;
    }
}