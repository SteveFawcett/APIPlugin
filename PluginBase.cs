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
    private const string PluginName = "APIPlugin";
    private const string PluginDescription = "REST API Plugin.";
    private const string Stanza = "API";

    public class PluginSettings
    {
        public GetCacheDataDelegate? GetCacheData;
    }

    public class PluginSettingsAccessor
    {
        public PluginSettings Current { get; set; } = new();
    }

    public class Startup
    {
        private readonly PluginSettingsAccessor _accessor;

        public Startup(PluginSettingsAccessor accessor)
        {
            _accessor = accessor;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton(_accessor); // Already registered in CreateWebHostBuilder
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseMvc();
        }
    }

    #region IPlugin Members

    public override GetCacheDataDelegate? GetCacheData
    {
        get => _webhost.Services.GetRequiredService<PluginSettingsAccessor>().Current.GetCacheData;
        set => _webhost.Services.GetRequiredService<PluginSettingsAccessor>().Current.GetCacheData = value;
    }

    private readonly IWebHost _webhost;
    private static readonly Image s_icon = Resources.green;
    private readonly ILogger<IPlugin> _logger;

    public PluginBase(IConfiguration configuration , ILogger<IPlugin> logger ) :
        base(configuration, null, s_icon, PluginName, Stanza, PluginDescription)
    {
        _logger = logger;
        _webhost = CreateWebHostBuilder( configuration.GetSection(Stanza) );
        _webhost.RunAsync();
        _logger.LogInformation(PluginDescription);
    }

        
    private IWebHost CreateWebHostBuilder(IConfiguration configuration)
    {
        var address = $"http://{configuration["server"]}:{configuration["port"]}";
        _logger.LogInformation("Starting API server on {0}" , address);

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

    #endregion
}