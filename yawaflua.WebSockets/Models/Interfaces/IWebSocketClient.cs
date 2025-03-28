using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;

namespace yawaflua.WebSockets.Models.Interfaces;

public interface IWebSocketClient
{
    /// <summary>
    /// ID of user
    /// </summary>
    public Guid Id { get; }
    /// <summary>
    /// Path, that user connects
    /// </summary>
    public string Path { get; }
    /// <summary>
    /// Connection info
    /// </summary>
    public ConnectionInfo? ConnectionInfo { get; }
    /// <summary>
    /// You can provides Items, like in auth middleware or any
    /// </summary>
    public IDictionary<object, object>? Items { get; set; }
    /// <summary>
    /// HttpRequest data
    /// </summary>
    public HttpRequest? HttpRequest { get; }
    /// <summary>
    /// You should`nt use it, but, its full work with user
    /// </summary>
    internal WebSocket webSocket { get; }
    /// <summary>
    /// Kick client
    /// </summary>
    /// <returns>null</returns>
    public Task Abort();
}