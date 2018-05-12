using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Windows.ServiceWrapper
{
    public class WindowsService : ServiceBase
    {
        private readonly System.ComponentModel.IContainer components;
        private IDaemon _daemon;
        private Task _workTask;
        private CancellationTokenSource _cancellationTokenSource;

        public WindowsService()
        {
            components = new System.ComponentModel.Container();
        }

        protected override void OnStart(string[] startArgs)
        {
            var args = Environment.GetCommandLineArgs()
                .Skip(1)
                .ToArray();
            _cancellationTokenSource = new CancellationTokenSource();
            _daemon = new DaemonBuilder(args)
                .ConfigureLogging(options => { })
                .UseService<Daemon>()
                .Build();
            _workTask = _daemon.RunAsync(_cancellationTokenSource.Token);
        }

        protected override void OnStop()
        {
            _cancellationTokenSource.Cancel();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}