using BroadcastPluginSDK;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;

namespace APIPlugin
{
    public class Plugin : BroadcastPlugin , IPlugin
    {
        #region Private Fields
        #endregion

        #region IPlugin Members
        public override string Stanza => "API";

        private new IConfiguration? Configuration;

        public Plugin()
        {
            Name = "API Plugin";
            Description = "Plugin provides a REST API.";
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            Icon = Properties.Resources.red;
            Debug.WriteLine($"Plugin {Name} initialized with version {Version}");
        }
        public override bool AttachConfiguration<T>(T configuration)
        {
            var configSection = configuration as Dictionary<string, string?>;
            if (configSection == null)
            {
                Debug.WriteLine($"API Plugin Configuration section is not of type {typeof(T)}");
                return false;
            }
            Debug.WriteLine($"Attaching {Name} configuration to Plugin");
            var ConfigurationBuilder = new ConfigurationBuilder();
            ConfigurationBuilder.AddInMemoryCollection(configSection);
            Configuration = ConfigurationBuilder.Build();
            return true;
        }

        public override void Start()
        {
            if(Configuration == null)
            {
                Debug.WriteLine("API Plugin Configuration is not set. Please attach configuration before starting the plugin.");
                return;
            }
            CreateWebHostBuilder( Configuration ).Build().RunAsync();
        }

        private static IWebHostBuilder CreateWebHostBuilder(IConfiguration Configuration)
        {
            var address = $"http://{Configuration["server"]}:{Configuration["port"]}";
            Debug.WriteLine($"Starting API server on {address}");

            return WebHost.CreateDefaultBuilder([])
                .UseUrls( address )
                .UseStartup<Startup>() ; 
        }

        #endregion
        internal class Startup
        {
            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }
            public IConfiguration Configuration { get; }
            public void ConfigureServices(IServiceCollection services)
            {
                 services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            }
            public void Configure(IApplicationBuilder app)
            {
               
                app.UseDeveloperExceptionPage();
                
                app.UseMvc();
            }
        }
    }

}