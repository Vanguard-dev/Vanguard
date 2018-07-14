using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vanguard.ServerManager.Node.Abstractions;
using Vanguard.ServerManager.Node.Core;

namespace Vanguard.ServerManager.Node
{
    public class AuthService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly LocalCredentialsProvider _localCredentialsProvider;
        private readonly Authenticator _authenticator;
        private bool _authenticationDone;
        private Timer _refreshTimer;

        public AuthService(ILoggerFactory loggerFactory, LocalCredentialsProvider localCredentialsProvider, Authenticator authenticator)
        {
            _logger = loggerFactory.CreateLogger<AuthService>();
            _localCredentialsProvider = localCredentialsProvider;
            _authenticator = authenticator;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _refreshTimer = new Timer(OnTick, null, 0, -1);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _refreshTimer.Dispose();
            return Task.CompletedTask;
        }

        private async void OnTick(object state)
        {
            int expiresIn;
            if (!_authenticationDone)
            {
                _logger.LogInformation("Attempting to authenticate the session");
                var credentials = await _localCredentialsProvider.GetCredentialsAsync<UsernamePasswordCredentialsViewModel>("CoreConnectionCredentials");
                expiresIn = Convert.ToInt32(await _authenticator.AuthenticateAsync(credentials.Username, credentials.Password));
                _authenticationDone = true;
            }
            else
            {
                _logger.LogInformation("Attempting to refresh the session");
                expiresIn = Convert.ToInt32(await _authenticator.RefreshAsync());
            }
            _logger.LogDebug("Successful. Refreshing in {0} seconds", expiresIn);
            _refreshTimer = new Timer(OnTick, null, expiresIn * 1000, -1);
        }
    }
}