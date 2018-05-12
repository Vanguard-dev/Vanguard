using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Windows.ServiceWrapper
{
    public class Daemon : IDaemon
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private ServiceHost _host;

        public Daemon(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Daemon>();
            _configuration = configuration;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var serviceDefinition = new ServiceDefinition();
            _configuration.Bind("ServiceDefinition", serviceDefinition);
            _host = new ServiceHost(serviceDefinition, _loggerFactory);
            _host.Start();

            while (!cancellationToken.IsCancellationRequested && _host.IsRunning)
            {
                // TODO: Implement scheduled actions for hosted services
                await Task.Delay(1000, cancellationToken);
            }

            _host.Stop();
        }

        public void Kill()
        {
            _host.Kill();
        }
    }
}