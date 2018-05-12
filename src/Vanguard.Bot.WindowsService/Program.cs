using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Bot.WindowsService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Any(t => t == "--foreground") || args.Any(t => t == "-f"))
            {
                await new DaemonBuilder(args)
                    .UseStartup<Startup>()
                    .UseService<BotManager>()
                    .Build()
                    .RunAsync();
            }
            else
            {
                try
                {
                    ServiceBase.Run(new ServiceBase[]
                    {
                        new BotService()
                    });
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
