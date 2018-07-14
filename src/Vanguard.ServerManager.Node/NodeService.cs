using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Vanguard.ServerManager.Node
{
    public class NodeService : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;

        public NodeService(ILoggerFactory loggerFactory)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = loggerFactory.CreateLogger<NodeService>();
        }

        public async Task StartAsync(CancellationToken providedCancellationToken)
        {
            if (providedCancellationToken.CanBeCanceled)
            {
                providedCancellationToken.Register(() => _cancellationTokenSource.Cancel());
            }
            var cancellationToken = _cancellationTokenSource.Token;

            _logger.LogInformation("Starting NodeService");

            // TODO: Start watchdog thread for game servers
            // TODO: Start MQ listener for tasks

            await Task.Delay(-1, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping NodeService");
            return Task.CompletedTask;
        }
    }
}