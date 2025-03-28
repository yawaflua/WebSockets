using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using yawaflua.WebSockets.Core;
using yawaflua.WebSockets.Core.Middleware;
using yawaflua.WebSockets.Models.Interfaces;

namespace yawaflua.WebSockets;

public static class ServiceBindings
{
    public static IServiceCollection SettingUpWebSockets(this IServiceCollection isc)
    {
        isc.AddSingleton<WebSocketRouter>();
        isc.AddScoped<IWebSocketManager, WebSocketManager>();
        isc.AddSingleton<WebSocketMiddleware>();
        return isc;
    }

    public static IApplicationBuilder ConnectWebSockets(this IApplicationBuilder iab)
    {
        iab.UseWebSockets();
        iab.UseMiddleware<WebSocketMiddleware>();
        return iab;
    }
}