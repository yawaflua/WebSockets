using System.Net.WebSockets;
using yawaflua.WebSockets;
using yawaflua.WebSockets.Core;

namespace Examples;

class Program
{
    static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureLogging(k => k.AddConsole().AddDebug())
            .ConfigureWebHost(k =>
            {
                k.UseKestrel(l => l.ListenAnyIP(80));
                k.UseStartup<Startup>();
            })
            .RunConsoleAsync();
    }
}

internal class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.SettingUpWebSockets();
        services.AddRouting();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddHttpLogging();
        services.AddSingleton<TestWebSocketServer>();
        services.AddSingleton<ChatController>();
        services.AddSingleton(new WebSocketConfig()
        {
            OnOpenHandler = async (socket, _) =>
            {
                if (socket.WebSocketManager.GetAllClients().Count(k =>
                        Equals(k.ConnectionInfo!.RemoteIpAddress!.MapToIPv4(), 
                            socket.Client.ConnectionInfo!.RemoteIpAddress!.MapToIPv4())) >= 3)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Too many users");
                }
                Console.WriteLine($"{socket.Client.Id} has been connected to {socket.Client.Path}");
            }
        });
        services.AddScoped<IConfiguration>(_ => new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .Build());
    }

    public static void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        
        app.UseWebSocketWithSwagger();
        app.UseHttpLogging();
        app.UseWelcomePage();
        
    }
}