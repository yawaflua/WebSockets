using System.Linq.Expressions;
using System.Net.WebSockets;

namespace yawaflua.WebSockets.Models.Interfaces;

public interface IWebSocketManager
{
    /// <summary>
    /// Broadcast message to all users
    /// </summary>
    /// <param name="selector">selector of users</param>
    /// <param name="message">message, broadcasted to all users</param>
    /// <param name="messageType">type of message</param>
    /// <param name="cts">cancellation token</param>
    /// <returns></returns>
    public Task Broadcast(Func<IWebSocketClient, bool> selector,string message, WebSocketMessageType messageType = WebSocketMessageType.Text, CancellationToken cts = default);
    /// <summary>
    /// Provides list of clients connected to host
    /// </summary>
    /// <returns></returns>
    public List<IWebSocketClient> GetAllClients();
    /// <summary>
    /// Send to specific user data
    /// Analog of <c>Broadcast(k => k.Id == ID, message)</c>
    /// </summary>
    /// <param name="id">Id of user</param>
    /// <param name="message">message to user</param>
    /// <param name="messageType">type of message</param>
    /// <param name="cts">cancellation token</param>
    /// <returns></returns>
    public Task SendToUser(Guid id, string message, WebSocketMessageType messageType, CancellationToken cts);
}