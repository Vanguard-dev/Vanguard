using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Windows.ServiceWrapper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Any(t => t == "--foreground") || args.Any(t => t == "-f"))
            {
                try
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    var daemon = new DaemonBuilder(args)
                        .UseService<Daemon>()
                        .Build();
                    var workTask = daemon.RunAsync(cancellationTokenSource.Token);
                    Console.ReadKey();
                    cancellationTokenSource.Cancel();
                    await workTask;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("wtf");
                }
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
