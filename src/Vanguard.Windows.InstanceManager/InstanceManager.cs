using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Windows.InstanceManager
{
    public class InstanceManager : IDaemon
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public InstanceManager(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<InstanceManager>();
            _configuration = configuration;
        }

        public void Run()
        {
            RunAsync(new CancellationToken()).GetAwaiter().GetResult();
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var serviceHosts = new List<ServiceHost>();
            var servicesToHost = _configuration.GetSection("Services").GetChildren();
            foreach (var entry in servicesToHost)
            {
                var serviceName = entry.Key;
                var serviceDefinition = new ServiceDefinition();
                _configuration.GetSection("Services").Bind(entry.Key, serviceDefinition);
                var host = new ServiceHost(serviceName, serviceDefinition, _loggerFactory);
                serviceHosts.Add(host);
                host.Start();
            }

            // TODO: API for interacting with the hosted services
            // TODO: Per host logging for debugging and health checks

            while (!cancellationToken.IsCancellationRequested)
            {
                // TODO: Implement scheduled actions for hosted services
                await Task.Delay(1000, cancellationToken);
            }

            foreach (var host in serviceHosts)
            {
                host.Kill();
            }
        }
    }
}