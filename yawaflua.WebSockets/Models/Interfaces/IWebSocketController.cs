using Microsoft.AspNetCore.Http;
using yawaflua.WebSockets.Core;

namespace yawaflua.WebSockets.Models.Interfaces;

internal interface IWebSocketController
{
    Task OnMessageAsync(WebSocket webSocket, HttpContext httpContext);
}