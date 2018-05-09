using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Windows.InstanceManager
{
    public class InstanceManager : IDaemon
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public InstanceManager(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<InstanceManager>();
            _configuration = configuration;
        }

        public void Run()
        {
            RunAsync(new CancellationToken()).GetAwaiter().GetResult();
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            for (uint i = 0; i >= 0; i++)
            {
                _logger.LogDebug("Cycle start bitches!");
                await Task.Delay(1000, cancellationToken);
                _logger.LogTrace("Cycle done bitches!");
            }
        }
    }
}