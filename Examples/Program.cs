using System.Net.WebSockets;
using yawaflua.WebSockets;
using yawaflua.WebSockets.Core;
using yawaflua.WebSockets.Models.Interfaces;

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
    public void ConfigureServices(IServiceCollection services)
    {
        services.SettingUpWebSockets();
        services.AddRouting();
        services.AddHttpLogging();
        services.AddSingleton<TestWebSocketServer>();
        services.AddSingleton<ChatController>();
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
        });
        services.AddScoped<IConfiguration>(k => new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .Build());
    }

    public void Configure(IApplicationBuilder app)
    {
        app.ConnectWebSockets();
        app.UseRouting();
        app.UseHttpLogging();
        app.UseWelcomePage();
        
    }
}