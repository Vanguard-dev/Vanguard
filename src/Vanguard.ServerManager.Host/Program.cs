using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vanguard.Daemon.Abstractions;
using Vanguard.Daemon.Windows;

namespace Vanguard.ServerManager.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var daemonHost = new DaemonHost()
                .UseService<Daemon>();

            if (args.Any(t => t == "--foreground") || args.Any(t => t == "-f"))
            {
                try
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    var workTask = daemonHost.RunAsync(cancellationTokenSource.Token);
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
                    daemonHost.RunAsService();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
