using System.Linq.Expressions;
using System.Net.WebSockets;

namespace yawaflua.WebSockets.Models.Interfaces;

public interface IWebSocketManager
{
    public Task Broadcast(Func<IWebSocketClient, bool> selector,string message, WebSocketMessageType messageType = WebSocketMessageType.Text, CancellationToken cts = default);
    public List<IWebSocketClient> GetAllClients();
    public Task SendToUser(Guid id, string message, WebSocketMessageType messageType, CancellationToken cts);
}