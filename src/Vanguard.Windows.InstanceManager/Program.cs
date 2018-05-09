using System;
using System.Linq;
using System.ServiceProcess;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Windows.InstanceManager
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any(t => t == "--foreground") || args.Any(t => t == "-f"))
            {
                new DaemonBuilder(args)
                    .ConfigureLogging(options => { })
                    .UseStartup<Startup>()
                    .UseService<InstanceManager>()
                    .Build()
                    .Run();
            }
            else
            {
                try
                {
                    ServiceBase.Run(new ServiceBase[]
                    {
                        new ManagerService()
                    });
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
