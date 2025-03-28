using yawaflua.WebSockets;

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