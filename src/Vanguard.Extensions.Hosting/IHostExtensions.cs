using System.ServiceProcess;
using Microsoft.Extensions.Hosting;

namespace Vanguard.Extensions.Hosting
{
    public static class HostingExtensions
    {
        public static void RunAsService(this IHost host)
        {
            // TODO: Add support for systemd services
            ServiceBase.Run(new ServiceBase[]
            {
                new WindowsService(host)
            });
        }

        private class WindowsService : ServiceBase
        {
            private readonly System.ComponentModel.IContainer components;
            private readonly IHost _host;

            public WindowsService(IHost host)
            {
                _host = host;
                components = new System.ComponentModel.Container();
            }

            protected override void OnStart(string[] startArgs)
            {
                _host.Run();
            }

            protected override void OnStop()
            {
                _host.StopAsync().GetAwaiter().GetResult();
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