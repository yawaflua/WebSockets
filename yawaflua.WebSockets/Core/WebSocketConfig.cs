using Microsoft.AspNetCore.Http;
using yawaflua.WebSockets.Models.Interfaces;

namespace yawaflua.WebSockets.Core;

public class WebSocketConfig
{
    public Func<IWebSocket, HttpContext, Task>? OnOpenHandler { get; set; } = null;

}