using System;
using System.Linq;
using System.ServiceProcess;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Bot.WindowsService
{
    public class BotService : ServiceBase
    {
        private readonly System.ComponentModel.IContainer components;

        public BotService()
        {
            components = new System.ComponentModel.Container();
            ServiceName = "VanguardBotService";
        }

        protected override void OnStart(string[] startArgs)
        {
            var args = Environment.GetCommandLineArgs()
                .Skip(1)
                .ToArray();
            new DaemonBuilder(args)
                .ConfigureLogging(options => { })
                .UseStartup<Startup>()
                .UseService<BotManager>()
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