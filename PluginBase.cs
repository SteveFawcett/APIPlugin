using APIPlugin.Properties;
using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace APIPlugin;

public class PluginBase : BroadcastPluginBase 
{
    private const string STANZA = "API";
    private readonly IWebHost? _webhost;
    private static readonly Image s_icon = Resources.green;
    private readonly ILogger<IPlugin>? _logger;

    public class PluginSettings
    {
        public GetCacheDataDelegate? GetCacheData;
    }

    public class PluginSettingsAccessor
    {
        public PluginSettings Current { get; set; } = new();
    }

    public class Startup( PluginSettingsAccessor accessor ) 
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton( accessor); // Already registered in CreateWebHostBuilder
        }

        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseMvc();
        }
    }

    #region IPlugin Members

    public PluginBase() :base() {}

    public PluginBase(IConfiguration configuration , ILogger<IPlugin> logger ) :
        base(configuration, null, s_icon,  STANZA)
    {
        _logger = logger;
        
        _webhost = CreateWebHostBuilder(configuration.GetSection(STANZA));

        Task.Run(() => InitializeWebHost());
    }

    private void InitializeWebHost()
    {
        _webhost.Run();
    }

    private IWebHost CreateWebHostBuilder(IConfiguration configuration)
    {
        var address = $"http://{configuration["server"]}:{configuration["port"]}";
        _logger?.LogInformation("Starting API server on {address}" , address);

        return WebHost.CreateDefaultBuilder()
            .UseUrls(address)
            .ConfigureServices(services =>
            {
                services.AddSingleton(configuration);
                services.AddSingleton<PluginSettingsAccessor>();
                services.AddMvc();
            })
            .UseStartup<Startup>()
            .Build();
    }

    public override GetCacheDataDelegate? GetCacheData
    {
        get => _webhost?.Services.GetRequiredService<PluginSettingsAccessor>().Current.GetCacheData;
        set
        {
            if (_webhost == null)
            {
                Debug.WriteLine("Web host is not initialized. Cannot set GetCacheData.");
                return;
            }
            _webhost.Services.GetRequiredService<PluginSettingsAccessor>().Current.GetCacheData = value;
        }
    }
    #endregion
}