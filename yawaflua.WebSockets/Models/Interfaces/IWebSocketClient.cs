using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;

namespace yawaflua.WebSockets.Models.Interfaces;

public interface IWebSocketClient
{
    public Guid Id { get; }
    public string Path { get; }
    public ConnectionInfo? ConnectionInfo { get; }
    public IDictionary<object, object>? Items { get; set; }
    public HttpRequest? HttpRequest { get; }
    internal WebSocket webSocket { get; }
    public Task Abort();
}