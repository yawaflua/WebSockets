using System.Net.WebSockets;
using yawaflua.WebSockets.Core;

namespace yawaflua.WebSockets.Models.Interfaces;

public interface IWebSocket : IDisposable
{
    WebSocketState State { get; }
    IWebSocketManager WebSocketManager { get; }
    WebSocketCloseStatus? CloseStatus { get; }
    string? SubProtocol { get; }
    string? CloseStatusDescription { get; }
    string? Message { get; }
    WebSocketMessageType? MessageType { get; }
    IWebSocketClient Client { get; }

    Task SendAsync(string message, 
        WebSocketMessageType messageType = WebSocketMessageType.Text,
        CancellationToken cancellationToken = default);

    Task CloseAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure,
        string? reason = null,
        CancellationToken cancellationToken = default);

    void Abort();
}