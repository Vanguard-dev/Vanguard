using System;
using System.Linq;
using System.Threading.Tasks;
using Vanguard.Daemon.Abstractions;
using Vanguard.Daemon.Windows;

namespace Vanguard.Bot.WindowsService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var daemonHost = new DaemonHost()
                .UseStartup<Startup>()
                .UseService<BotManager>();

            if (args.Any(t => t == "--foreground") || args.Any(t => t == "-f"))
            {
                await daemonHost.RunAsync();
            }
            else
            {
                try
                {
                    daemonHost.RunAsService();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
