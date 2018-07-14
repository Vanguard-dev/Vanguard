using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Vanguard.Daemon.Abstractions
{
    public class DaemonHost
    {
        private readonly DaemonBuilder _builder;
        private IDaemon _daemon;
        public IDaemon Daemon => _daemon ?? (_daemon = _builder.Build());

        public ServiceProvider Services
        {
            get
            {
                if (_daemon == null)
                {
                    _daemon = _builder.Build();
                }

                return _builder.Services;
            }
        }

        public DaemonHost()
        {
            var args = Environment.GetCommandLineArgs()
                .Skip(1)
                .ToArray();

            _builder = new DaemonBuilder(args);
        }

        public DaemonHost UseStartup<T>() where T : class, IStartup, new()
        {
            _builder.UseStartup<T>();

            return this;
        }

        public DaemonHost UseService<T>() where T : class, IDaemon
        {
            _builder.UseService<T>();

            return this;
        }

        public Task RunAsync(CancellationToken cancellationToken = default) => Daemon.RunAsync(cancellationToken);
    }
}