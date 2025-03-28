using Microsoft.AspNetCore.Http;
using yawaflua.WebSockets.Core;

namespace yawaflua.WebSockets.Models.Interfaces;

internal interface IWebSocketController
{
    /// <summary>
    /// Example of working with IWebSocketController
    /// </summary>
    /// <param name="webSocket"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    Task OnMessageAsync(IWebSocket webSocket, HttpContext httpContext);
}