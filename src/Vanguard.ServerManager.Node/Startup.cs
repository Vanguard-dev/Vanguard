using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vanguard.Daemon.Abstractions;
using Vanguard.ServerManager.Node.Abstractions;
using Vanguard.ServerManager.Node.Abstractions.Windows;

namespace Vanguard.ServerManager.Node
{
    public class Startup : IStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<LocalCredentialsProvider>();

            if ((string) RegistryHelper.GetVanguardKey().GetValue("NodeInstalled") == "yes")
            {
                var options = new NodeOptions
                {
                    IsInstalled = true,
                    CoreConnectionHostname = RegistryHelper.GetVanguardKey().GetValue("CoreConnectionHostname") as string,
                    CoreConnectionNoSsl =  (string)RegistryHelper.GetVanguardKey().GetValue("CoreConnectionNoSsl") == "yes",
                    CoreConnectionIgnoreSslWarnings = (string)RegistryHelper.GetVanguardKey().GetValue("CoreConnectionIgnoreSslWarnings") == "yes"
                };
                services.AddSingleton(options);
            }
        }

        public void ConfigureApp(IServiceProvider services, IConfiguration configuration)
        {
        }
    }
}