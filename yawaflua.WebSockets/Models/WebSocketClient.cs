using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using yawaflua.WebSockets.Models.Interfaces;

namespace yawaflua.WebSockets.Models;

internal class WebSocketClient : IWebSocketClient
{
    private HttpContext HttpContext { get; set; }
    public Guid Id { get; } = Guid.NewGuid();
    public string Path { get; }
    public ConnectionInfo ConnectionInfo { get => HttpContext.Connection; }
    public IDictionary<object, object> Items
    {
        get => HttpContext.Items;
        set => HttpContext.Items = (value);
    }

    public HttpRequest HttpRequest { get => HttpContext.Request; }
    public WebSocket webSocket { get; }

    internal WebSocketClient(HttpContext httpContext, WebSocket webSocket, string path)
    {
        this.webSocket = webSocket;
        Path = path;
        this.HttpContext = httpContext;
    }
    
    public Task Abort()
    {
        webSocket.Abort();
        return Task.CompletedTask;
    }
}