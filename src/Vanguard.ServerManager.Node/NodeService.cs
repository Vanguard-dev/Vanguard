using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Node.Core;

namespace Vanguard.ServerManager.Node
{
    public class NodeService : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;
        private readonly Authenticator _authenticator;
        private readonly NodeOptions _options;

        public NodeService(ILoggerFactory loggerFactory, Authenticator authenticator, NodeOptions options)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = loggerFactory.CreateLogger<NodeService>();
            _authenticator = authenticator;
            _options = options;
        }

        public async Task StartAsync(CancellationToken providedCancellationToken)
        {
            if (providedCancellationToken.CanBeCanceled)
            {
                providedCancellationToken.Register(() => _cancellationTokenSource.Cancel());
            }
            var cancellationToken = _cancellationTokenSource.Token;

            _logger.LogInformation("Starting NodeService");

            while (!_authenticator.IsAuthenticated)
            {
                _logger.LogDebug("Waiting for authenticator to finish authenticating");
                await Task.Delay(10000, cancellationToken);
            }

            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"{_options.ApiRoot}/hubs/node", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_authenticator.AccessToken);
                })
                .Build();

            // TODO: Start listening for node tasks

            await hubConnection.StartAsync(cancellationToken);

            // TODO: Start watchdog thread for game servers
            // TODO: Start MQ listener for tasks

            await Task.Delay(-1, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping NodeService");
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}