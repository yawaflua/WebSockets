using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

[SuppressMessage("ReSharper", "AsyncVoidLambda")]
public class WebSocketRouter
{
    internal static readonly ConcurrentDictionary<string, Func<WebSocket, HttpContext, Task>> Routes = new();
    internal static readonly List<IWebSocketClient> Clients = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebSocketRouter> _logger;
    private readonly WebSocketConfig? _webSocketConfig;
    public WebSocketRouter(IServiceProvider serviceProvider, ILogger<WebSocketRouter> logger, WebSocketConfig? webSocketConfig = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _webSocketConfig = webSocketConfig;
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
                        parameters[0].ParameterType != typeof(IWebSocket) ||
                        parameters[1].ParameterType != typeof(HttpContext) ||
                        func.ReturnType != typeof(Task))
                    {
                        _logger.LogCritical($"Invalid handler signature in {type.Name}.{func.Name}");
                        throw new InvalidOperationException(
                            $"Invalid handler signature in {type.Name}.{func.Name}");
                    }

                    if (func.IsStatic)
                    {
                        var delegateFunc = (Func<WebSocket, HttpContext, Task>)Delegate.CreateDelegate(
                            typeof(Func<WebSocket, HttpContext, Task>), 
                            func
                        );
                        if (!Routes.TryAdd(parentAttributeTemplate, delegateFunc))
                        {
                            _logger.LogCritical($"Error registered whilest adds new route: {parentAttributeTemplate}");
                            throw new InvalidOperationException(
                                $"Error registered whilest adds new route: {parentAttributeTemplate}");
                        }
                    }
                    else
                    {
                        if (!Routes.TryAdd(parentAttributeTemplate, async (ws, context) =>
                            {
                                var instance = context.RequestServices.GetRequiredService(type);
                                await (Task)func.Invoke(instance, new object[] { ws, context })!;
                            }))
                        {
                            _logger.LogCritical($"Error registered whilest adds new route: {parentAttributeTemplate}");
                            throw new InvalidOperationException(
                                $"Error registered whilest adds new route: {parentAttributeTemplate}");
                        }
                    }
                }
                else
                {
                    foreach (var method in methods)
                    {
                        var attribute =
                            (WebSocketAttribute)method.GetCustomAttributes(typeof(WebSocketAttribute), false).First();
                        
                        var key = parentAttributeTemplate+attribute.Template;

                        if (Routes.ContainsKey(key))
                        {
                            Debug.WriteLine(Routes);
                            _logger.LogCritical($"Duplicate route error: {key}");
                            throw new InvalidOperationException(
                                $"Duplicate route error: {key}");
                        }
                        
                        if (method.IsStatic)
                        {
                            var delegateFunc = (Func<WebSocket, HttpContext, Task>)Delegate.CreateDelegate(
                                typeof(Func<WebSocket, HttpContext, Task>), 
                                method
                            );
                            if (!Routes.TryAdd(key, delegateFunc))
                            {
                                _logger.LogCritical($"Error registered whilest adds new route: {key}");
                                throw new InvalidOperationException(
                                    $"Error registered whilest adds new route: {key}");
                            }
                        }
                        else
                        {
                            if (!Routes.TryAdd(key, async (ws, context) =>
                                {
                                    var instance = context.RequestServices.GetRequiredService(type);
                                    await (Task)method.Invoke(instance, new object[] { ws, context })!;
                                }))
                            {
                                _logger.LogCritical($"Error registered whilest adds new route: {key}");
                                throw new InvalidOperationException(
                                    $"Error registered whilest adds new route: {key}");
                            }
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
            _logger.LogCritical("Error when parsing attributes from assemblies: {ex}", ex);
            Debug.WriteLine(ex);
            Debug.WriteLine(Routes);
            throw new Exception("Error when parsing attributes from assemblies", ex);
        }

#if DEBUG
        _logger.LogDebug("Routes:");
        foreach (var route in Routes)
        {
            _logger.LogDebug("Key:FuncName => {k}:{f}", route.Key, route.Value.Method.Name);
        }
#endif
    }

    internal async Task HandleRequest(HttpContext context, CancellationToken cts = default)
    {
        try
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;
            
            var path = context.Request.Path.Value;

            if (path != null && Routes.TryGetValue(path, out var handler))
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await Task.Run(async () =>
                {
                    IWebSocketClient client = null!;
                    var webSocketManager = new WebSocketManager();
                    try
                    {
                        client = new WebSocketClient(context, webSocket, path);
                        Clients.Add(client);
                        
                        await Task.Run(async () =>
                        {
                            if (_webSocketConfig?.OnOpenHandler != null)
                                await _webSocketConfig.OnOpenHandler(new WebSocket(webSocket, client, webSocketManager), context);
                        }, cts);
                        
                        var buffer = new byte[1024 * 4];
                        while (webSocket.State == WebSocketState.Open)
                        {
                            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts);
                            if (result.MessageType != WebSocketMessageType.Close)
                                await handler(
                                    new WebSocket(
                                        webSocket,
                                        client,
                                        webSocketManager,
                                        Encoding.UTF8.GetString(buffer, 0, result.Count),
                                        result), context);
                            else
                                Clients.Remove(client);
                        }
                        
                        if (Clients.Any(k => k.Id == client.Id))
                            Clients.Remove(client);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error with handling request: {ex}", ex);
                        await Task.Run(async () =>
                        {
                            if (_webSocketConfig?.OnErrorHandler != null)
                                await _webSocketConfig.OnErrorHandler(ex, new WebSocket(webSocket, client, webSocketManager), context);
                        }, cts);
                        
                    }

                }, cts);
            }
            else
            { 
                context.Response.StatusCode = 404;
                throw new KeyNotFoundException("Path not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error when handle request {context.Connection.RemoteIpAddress}: {ex}");
            if (_webSocketConfig!.OnConnectionErrorHandler != null)
                await _webSocketConfig.OnConnectionErrorHandler(ex, context);
        }
    }
}