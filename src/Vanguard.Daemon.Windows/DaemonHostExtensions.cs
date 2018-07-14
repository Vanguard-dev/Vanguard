using System.ServiceProcess;
using System.Threading;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Daemon.Windows
{
    public static class DaemonHostExtensions
    {
        public static void RunAsService(this DaemonHost host)
        {
            // TODO: Add support for systemd services
            ServiceBase.Run(new ServiceBase[]
            {
                new WindowsService(host.Daemon)
            });
        }

        private class WindowsService : ServiceBase
        {
            private readonly System.ComponentModel.IContainer components;
            private readonly IDaemon _daemon;
            private CancellationTokenSource _cancellationTokenSource;

            public WindowsService(IDaemon daemon)
            {
                _daemon = daemon;
                components = new System.ComponentModel.Container();
            }

            protected override void OnStart(string[] startArgs)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _daemon.RunAsync(_cancellationTokenSource.Token);
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
}