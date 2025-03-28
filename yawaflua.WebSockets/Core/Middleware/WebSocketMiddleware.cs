using Microsoft.AspNetCore.Http;

namespace yawaflua.WebSockets.Core.Middleware;

public class WebSocketMiddleware : IMiddleware
{
    private readonly WebSocketRouter _router;

    public WebSocketMiddleware(WebSocketRouter router)
    {
        _router = router;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await _router.HandleRequest(context);
        }
        else
        {
            await next(context);
        }
    }
}