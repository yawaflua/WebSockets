using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using yawaflua.WebSockets.Attributes;
using yawaflua.WebSockets.Models;
using yawaflua.WebSockets.Models.Abstracts;
using yawaflua.WebSockets.Models.Interfaces;

namespace yawaflua.WebSockets.Core;

public class WebSocketRouter
{
    internal static readonly Dictionary<string, Func<WebSocket, HttpContext, Task>> Routes = new();
    internal static readonly List<IWebSocketClient> Clients = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebSocketRouter> _logger;
    private readonly WebSocketConfig WebSocketConfig;
    public WebSocketRouter(IServiceProvider serviceProvider, ILogger<WebSocketRouter> logger, WebSocketConfig webSocketConfig)
    {
        _serviceProvider = serviceProvider;
        this._logger = logger;
        WebSocketConfig = webSocketConfig;
        DiscoverHandlers();
        Task.Run(() =>
        {
            Clients.ForEach(async l =>
            {
                await l.webSocket.SendAsync(ArraySegment<byte>.Empty, WebSocketMessageType.Binary,
                    WebSocketMessageFlags.EndOfMessage, default);
                await Task.Delay(TimeSpan.FromSeconds(10));
            });
        });
    }

    internal void DiscoverHandlers()
    {
        try
        {
            var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t.IsSubclassOf(typeof(IWebSocketController))
                    || t.IsSubclassOf(typeof(WebSocketController))
                    || t.IsInstanceOfType(typeof(WebSocketController))
                    || t.IsInstanceOfType(typeof(IWebSocketController))
                );
            using var scope = _serviceProvider.CreateScope();
            foreach (var type in handlerTypes.Where(k => k.GetMethods().Length > 0))
            {
                var parentAttributeTemplate =
                    new PathString((type.GetCustomAttribute(typeof(WebSocketAttribute)) as WebSocketAttribute)?.Template ?? "/");
                var methods = type.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(WebSocketAttribute), false).Length > 0).ToList();
                

                if (methods.Count == 0 && type.GetMethods().Any(k => k.Name.StartsWith("OnMessage")))
                {
                    var func = type.GetMethods()
                        .First(k => k.Name.StartsWith("OnMessage"));
    
                    var parameters = func.GetParameters();
                    if (parameters.Length != 2 || 
                        parameters[0].ParameterType != typeof(WebSocket) ||
                        parameters[1].ParameterType != typeof(HttpContext) ||
                        func.ReturnType != typeof(Task))
                    {
                        throw new InvalidOperationException(
                            $"Invalid handler signature in {type.Name}.{func.Name}");
                    }

                    if (func.IsStatic)
                    {
                        var delegateFunc = (Func<WebSocket, HttpContext, Task>)Delegate.CreateDelegate(
                            typeof(Func<WebSocket, HttpContext, Task>), 
                            func
                        );
                        Routes.Add(parentAttributeTemplate, delegateFunc);
                    }
                    else
                    {
                        Routes.Add(parentAttributeTemplate, async (ws, context) => 
                        {
                            var instance = context.RequestServices.GetRequiredService(type);
                            await (Task)func.Invoke(instance, new object[] { ws, context })!;
                        });
                    }
                }
                else
                {
                    foreach (var method in methods)
                    {
                        var attribute =
                            (WebSocketAttribute)method.GetCustomAttributes(typeof(WebSocketAttribute), false).First();
                        if (method.IsStatic)
                        {
                            var delegateFunc = (Func<WebSocket, HttpContext, Task>)Delegate.CreateDelegate(
                                typeof(Func<WebSocket, HttpContext, Task>), 
                                method
                            );
                            Routes.Add(parentAttributeTemplate+attribute.Template, delegateFunc);
                        }
                        else
                        {
                            Routes.Add(parentAttributeTemplate+attribute.Template, async (ws, context) => 
                            {
                                var instance = context.RequestServices.GetRequiredService(type);
                                await (Task)method.Invoke(instance, new object[] { ws, context })!;
                            });
                        }
                    }
                }
                var constructors = type.GetConstructors();
                if (constructors.Length != 0)
                {
                    var parameters = constructors[0].GetParameters()
                        .Select(param => scope.ServiceProvider.GetRequiredService(param.ParameterType))
                        .ToArray();

                    constructors[0].Invoke(parameters);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(message:"Error when parsing attributes from assemblies: ", exception:ex);
        }
    }

    internal async Task HandleRequest(HttpContext context, CancellationToken cts = default)
    {
        try
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;
            
            var path = context.Request.Path.Value;

            if (Routes.TryGetValue(path, out var handler))
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await Task.Run(async () =>
                {
                    try
                    {
                        var client = new WebSocketClient(context, webSocket, path);
                        Clients.Add(client);
                        
                        await Task.Run(async () =>
                        {
                            if (WebSocketConfig.OnOpenHandler != null)
                                await WebSocketConfig.OnOpenHandler((webSocket as IWebSocket)!, context);
                        }, cts);
                        
                        var buffer = new byte[1024 * 4];
                        while (webSocket.State == WebSocketState.Open)
                        {
                            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts);
                            if (result.MessageType != WebSocketMessageType.Close)
                                await handler(
                                    new WebSocket(
                                        webSocket,
                                        result,
                                        Encoding.UTF8.GetString(buffer, 0, result.Count),
                                        client),
                                    context);
                            else
                                Clients.Remove(client);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(message:"Error with handling request: ",exception: ex);
                    }

                }, cts);
            }
            else
            { 
                context.Response.StatusCode = 404;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error when handle request {context.Connection.Id}: ");
        }
    }
}