using System;
using System.Linq;
using System.ServiceProcess;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Windows.ServiceWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any(t => t == "--foreground") || args.Any(t => t == "-f"))
            {
                var workTask = new DaemonBuilder(args)
                    .UseService<Daemon>()
                    .Build()
                    .RunAsync();
                workTask.GetAwaiter().GetResult();
            }
            else
            {
                try
                {
                    ServiceBase.Run(new ServiceBase[]
                    {
                        new WindowsService()
                    });
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
