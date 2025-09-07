using APIPlugin.Forms;
using APIPlugin.Properties;
using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static APIPlugin.Forms.InfoPanel;

namespace APIPlugin;

public class PluginBase : BroadcastPluginBase
{
    private const string STANZA = "API";
    private static readonly Image s_icon = Resources.green;
    private readonly ILogger<IPlugin>? _logger;
    private static InfoPanel? _infoPage;
    private IWebHost? _webHost;

    public class PluginSettings
    {
        public WriteMessageDelegate WriteMessage { get; set; } = null!;
        public GetCacheDataDelegate? GetCacheData { get; set; }
    }

    public class PluginSettingsAccessor
    {
        public PluginSettings Current { get; set; } = new();

        public PluginSettingsAccessor(WriteMessageDelegate writeMessage)
        {
            Current.WriteMessage = writeMessage;
        }
    }

    public PluginBase() : base() { }

    public PluginBase(IConfiguration configuration, ILogger<IPlugin> logger)
        : base(configuration, CreatePage(), s_icon, STANZA)
    {
        _logger = logger;
        _logger.LogInformation("API Plugin initializing...");

        _webHost = CreateWebHost(configuration.GetSection(STANZA));
        Task.Run(() => StartWebHost()); // Fire-and-forget
    }

    private static IInfoPage CreatePage()
    {
        _infoPage = new InfoPanel();
        return _infoPage as IInfoPage;
    }

    private void StartWebHost()
    {
        try
        {
            _webHost?.Start();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start API web host.");
        }
    }

    private IWebHost CreateWebHost(IConfiguration configuration)
    {
        var address = $"http://{configuration["server"]}:{configuration["port"]}";
        _logger?.LogInformation("Starting API server on {address}", address);

        return new WebHostBuilder()
            .UseKestrel()
            .UseUrls(address)
            .ConfigureServices(services =>
            {
                services.AddMvc();
                services.AddSingleton(configuration);
                services.AddSingleton<PluginSettingsAccessor>(sp => new PluginSettingsAccessor(_infoPage.WriteMessage));
            })
            .Configure(app =>
            {
                var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
                if (env.IsDevelopment())
                    app.UseDeveloperExceptionPage();
                app.Use(async (context, next) =>
                {
                    var sw = Stopwatch.StartNew();
                    await next();
                    sw.Stop();
                    var logger = context.RequestServices.GetService<ILogger<PluginBase>>();
                    logger?.LogInformation("Request to {Path} took {Elapsed}ms", context.Request.Path, sw.ElapsedMilliseconds);
                });

                app.UseMvc();
            })
            .Build();
    }

    public override GetCacheDataDelegate? GetCacheData
    {
        get => _webHost?.Services.GetRequiredService<PluginSettingsAccessor>().Current.GetCacheData;
        set
        {
            if (_webHost == null)
            {
                Debug.WriteLine("Web host is not initialized. Cannot set GetCacheData.");
                return;
            }

            _webHost.Services.GetRequiredService<PluginSettingsAccessor>().Current.GetCacheData = value;
        }
    }
}
