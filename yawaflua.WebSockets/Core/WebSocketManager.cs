using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Text;
using yawaflua.WebSockets.Models.Interfaces;

namespace yawaflua.WebSockets.Core;

internal class WebSocketManager : IWebSocketManager
{
    public async Task Broadcast(Func<IWebSocketClient, bool> selector, string message,
        WebSocketMessageType messageType = WebSocketMessageType.Text, CancellationToken cts = default)
    {
        foreach (var client in WebSocketRouter.Clients.Where(selector))
        {
            await client.webSocket.SendAsync(Encoding.UTF8.GetBytes(message),
                messageType,
                true,
                cts);
        }
    }

    public List<IWebSocketClient> GetAllClients()
    {
        return WebSocketRouter.Clients;
    }

    public async Task SendToUser(Guid id, string message, WebSocketMessageType messageType = WebSocketMessageType.Text, CancellationToken cts = default)
    {
        await WebSocketRouter.Clients.First(k => k.Id == id).webSocket.SendAsync(Encoding.UTF8.GetBytes(message),
            messageType,
            true,
            cts);
    }
}