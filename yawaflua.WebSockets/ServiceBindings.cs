using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using yawaflua.WebSockets.Core;
using yawaflua.WebSockets.Core.Middleware;
using yawaflua.WebSockets.Models.Interfaces;

namespace yawaflua.WebSockets;

public static class ServiceBindings
{
    public static IServiceCollection SettingUpWebSockets(this IServiceCollection isc,
        WebSocketConfig? socketOptions = null)
    {
        isc.AddSingleton<WebSocketRouter>();
        if (socketOptions != null) isc.AddSingleton(socketOptions);
        if (isc.All(k => k.ServiceType != typeof(WebSocketConfig)))
            isc.AddSingleton(new WebSocketConfig());
        isc.AddScoped<IWebSocketManager, WebSocketManager>();
        isc.AddSingleton<WebSocketMiddleware>();
        return isc;
    }

    public static IApplicationBuilder UseWebSocketWithSwagger(this IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.UseMiddleware<WebSocketMiddleware>();

        return app;
    }
}