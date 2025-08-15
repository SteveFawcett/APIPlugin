using APIPlugin.Properties;
using BroadcastPluginSDK;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Xml.Schema;

namespace APIPlugin;

public class PluginBase : BroadcastPluginBase
{
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMvc();
        }
    }
    
    #region IPlugin Members

    public override string Stanza => "API";
    public override string Name => "API PluginBase";

    public override GetCacheDataDelegate? GetCacheData
    {
        get => Webhost.Services.GetRequiredService<PluginSettingsAccessor>().Current.GetCacheData;
        set => Webhost.Services.GetRequiredService<PluginSettingsAccessor>().Current.GetCacheData = value;
    }

    private IWebHost Webhost;

    private new IConfiguration? Configuration;

    public PluginBase()
    {
        Name = "API PluginBase";
        Description = "PluginBase provides a REST API.";
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        Icon = Resources.red;
        Debug.WriteLine($"PluginBase {Name} initialized with version {Version}");
    }

    public override bool AttachConfiguration<T>(T configuration)
    {
        var configSection = configuration as Dictionary<string, string?>;
        if (configSection == null)
        {
            Debug.WriteLine($"API PluginBase Configuration section is not of type {typeof(T)}");
            return false;
        }

        Debug.WriteLine($"Attaching {Name} configuration to PluginBase");
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(configSection);
        Configuration = configurationBuilder.Build();
        return true;
    }



    public override string Start()
    {
        if (Configuration == null)
        {
            return ("API PluginBase Configuration is not set. Please attach configuration before starting the plugin.");
        }

        Webhost = CreateWebHostBuilder(Configuration);

        Webhost.RunAsync();
        return string.Empty;
    }

    private static IWebHost CreateWebHostBuilder(IConfiguration configuration)
    {
        var address = $"http://{configuration["server"]}:{configuration["port"]}";
        Debug.WriteLine($"Starting API server on {address}");

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