# New WebSocket routing system

## Features
- ASP.NET Core-style WebSocket routing 🛣️
- Method-based endpoint handlers (Like in ASP!) 🎯
- Simple integration with existing applications ⚡

## Installation
Add the NuGet package to your project:
```bash
dotnet add package yawaflua.WebSockets
```

## Quick Start

### 1. Create WebSocket Controller
```csharp
public class ChatController : WebSocketController
{
    [WebSocket("/chat")]
    public override async Task OnMessageAsync(
        IWebSocket webSocket, 
        HttpContext httpContext)
    {
        await webSocket.SendAsync("Message!");
    }
}
```

### 2. Configure Services
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddControllers()
        .SettingUpWebSockets(); // ← Add WebSocket routing
        
    services.AddSingleton<ChatController>();
}
```

### 3. Enable Middleware
```csharp
public void Configure(IApplicationBuilder app)
{
    app.ConnectWebSockets(); // ← Add WebSocket handling
    
    app.UseRouting();
    app.UseEndpoints(endpoints => endpoints.MapControllers());
}
```

## Advanced Usage

### Parameterized Routes
```csharp
public class NotificationsController : WebSocketController
{
    [WebSocket("/notifications/{userId}")]
    public override async Task OnMessageAsync(
        IWebSocket webSocket,
        HttpContext httpContext)
    {
        var userId = httpContext.Request.RouteValues["userId"];
        // Handle user-specific notifications
    }
}
```

### Method-Level Routes
```csharp
[WebSocket("/game")]
public class GameController : WebSocketController
{
    [WebSocket("join/{roomId}")]
    public async Task JoinRoom(IWebSocket webSocket, HttpContext context)
    {
        // Handle room joining
    }

    [WebSocket("leave/{roomId}")]
    public async Task LeaveRoom(IWebSocket webSocket, HttpContext context)
    {
        // Handle room leaving
    }
}
```
## Providing dependencies in controller
```csharp
[WebSocket("/chat")]
public class ChatController : WebSocketController
{
    public static DbContext dbContext;

    public ChatController(DbContext dbContext)
    {
        ChatController.dbContext = dbContext;
    }
    
    [WebSocket("join/{roomId}")]
    public async Task JoinRoom(WebSocket webSocket, HttpContext context)
    {
        await dbContext.Add(...);
        // Next your logic etc
    }

    [WebSocket("leave/{roomId}")]
    public async Task LeaveRoom(WebSocket webSocket, HttpContext context)
    {
        // Handle room leaving
    }
}
```

## Run any code on connection to WebSocket
```csharp
services.AddSingleton(new WebSocketConfig()
{
    OnOpenHandler = async (socket, context) =>
    {
        if (socket.WebSocketManager!.GetAllClients().Count(k =>
                Equals(k.ConnectionInfo!.RemoteIpAddress!.MapToIPv4(), 
                    socket.Client.ConnectionInfo!.RemoteIpAddress!.MapToIPv4())) >= 3)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Too many users");
        }
        Console.WriteLine($"{socket.Client.Id} has been connected to {socket.Client.Path}");
    }
})
```

## Work with c=connected users from any point at your code!
```csharp
public class MyCoolService 
{
    private IWebSocketManager _manager;
    public MyCoolService(IWebSocketManager manager)
    {
        _manager = manager;
    }
    
    public async Task DoSomething()
    {
        await _manager.Broadcast(k => k.Path == "/my/cool/endpoint", "Hello!");
    }
}

// DependencyInjection should provide IWebSocketManager to builder
services.AddSingleton<MyCoolService>(); 
```


## Lifecycle Management
1. **Connection** - Automatically handled by middleware
2. **Message Handling** - Implement `OnMessageAsync`
3. **Cleanup** - Dispose resources in `IDisposable` interface

## Best Practices
1. **Keep Controllers Light** - Move business logic to services
2. **Use Dependency Injection** - Inject services via constructor
3. **Handle Exceptions** - Wrap operations in try/catch blocks
4. **Manage State** - Use `HttpContext.Items` for request-scoped data

## Troubleshooting
**No Route Handling?**
- Verify controller registration in DI:
  ```csharp
  services.AddSingleton<YourController>();
  ```

**Connection Issues?**
- Ensure middleware order:
  ```csharp
  app.ConnectWebSockets(); // Must be before UseRouting/UseEndpoints
  ```

**Parameters Not Working?**
- Check route template syntax:
  ```csharp
  [WebSocket("/correct/{paramName}")] // ✓
  [WebSocket("/wrong/{param-name}")]  // ✗
  ```

## License
[Apache license - Free for commercial and personal use.](LICENSE)
