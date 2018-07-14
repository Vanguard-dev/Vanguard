using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vanguard.Extensions.Hosting;

namespace Vanguard.Bot.WindowsService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment}.json", true, true)
                        .AddEnvironmentVariables("VANGUARD_")
                        .AddCommandLine(args)
                        .Build();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddSingleton<IHostedService, BotService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                }).Build();

            if (args.Any(t => t == "--foreground") || args.Any(t => t == "-f"))
            {
                await host.RunAsync();
            }
            else
            {
                host.RunAsService();
            }
        }
    }
}