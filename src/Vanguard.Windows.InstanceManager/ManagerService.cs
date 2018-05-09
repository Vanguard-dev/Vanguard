using System;
using System.Linq;
using System.ServiceProcess;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Windows.InstanceManager
{
    public class ManagerService : ServiceBase
    {
        private readonly System.ComponentModel.IContainer components;

        public ManagerService()
        {
            components = new System.ComponentModel.Container();
            ServiceName = "VanguardInstanceManager";
        }

        protected override void OnStart(string[] startArgs)
        {
            var args = Environment.GetCommandLineArgs()
                .Skip(1)
                .ToArray();
            new DaemonBuilder(args)
                .ConfigureLogging(options => {})
                .UseStartup<Startup>()
                .UseService<InstanceManager>()
                .Build()
                .Run();
        }

        protected override void OnStop()
        {
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