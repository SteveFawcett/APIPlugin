using System.Diagnostics;
using APIPlugin.Properties;
using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Classes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    public PluginBase(IConfiguration configuration) : base(configuration, null, s_icon, "API plugin", "API", "REST API Provider")
    {

        _webhost = CreateWebHostBuilder( configuration.GetSection("API") );
        _webhost.RunAsync();
        Debug.WriteLine("API plugin Started");
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