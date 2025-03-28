using System.Net.WebSockets;
using System.Text;
using yawaflua.WebSockets.Models.Interfaces;

namespace yawaflua.WebSockets.Core;

public class WebSocket : IDisposable
{
    private readonly System.Net.WebSockets.WebSocket _webSocket;
    private readonly WebSocketReceiveResult? _webSocketReceiveResult;
    private readonly string? _message;

    public WebSocketState State => _webSocket.State;
    public WebSocketCloseStatus? CloseStatus => _webSocket.CloseStatus;
    public string? SubProtocol => _webSocket.SubProtocol;
    public string? CloseStatusDescription => _webSocket.CloseStatusDescription;
    public string? Message => _message;
    public WebSocketMessageType? MessageType => _webSocketReceiveResult?.MessageType;
    public IWebSocketClient Client;
    internal WebSocket(System.Net.WebSockets.WebSocket webSocket, WebSocketReceiveResult? webSocketReceiveResult, string? message, IWebSocketClient client)
    {
        _webSocket = webSocket;
        _webSocketReceiveResult = webSocketReceiveResult;
        _message = message;
        Client = client;
    }

    public async Task SendAsync(string m, WebSocketMessageType messageType = WebSocketMessageType.Text, CancellationToken cts = default)
    => await _webSocket.SendAsync(
            Encoding.UTF8.GetBytes(m),
            messageType,
            true,
            cts);

    public async Task CloseAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string? reason = null, CancellationToken cts = default)
    => await _webSocket.CloseAsync(closeStatus, reason, cts);
    

    
    public void Abort() => _webSocket.Abort();
    public void Dispose() => _webSocket.Dispose();
    
}