using Microsoft.AspNetCore.Http;
using yawaflua.WebSockets.Models.Interfaces;

namespace yawaflua.WebSockets.Core;

public class WebSocketConfig
{
    public Func<IWebSocket, HttpContext, Task>? OnOpenHandler { get; set; } = null;
    public Func<Exception, IWebSocket, HttpContext, Task>? OnErrorHandler { get; set; } = null;
    public Func<Exception, HttpContext, Task>? OnConnectionErrorHandler { get; set; } = null;

}