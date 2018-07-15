using System;
using System.Net.Http;
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
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;
        private readonly LocalCredentialsProvider _localCredentialsProvider;
        private readonly Authenticator _authenticator;
        private bool _authenticationDone;

        public AuthService(ILoggerFactory loggerFactory, LocalCredentialsProvider localCredentialsProvider, Authenticator authenticator)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = loggerFactory.CreateLogger<AuthService>();
            _localCredentialsProvider = localCredentialsProvider;
            _authenticator = authenticator;
        }

        public Task StartAsync(CancellationToken providedCancellationToken)
        {
            if (providedCancellationToken.CanBeCanceled)
            {
                providedCancellationToken.Register(() => _cancellationTokenSource.Cancel());
            }
            var cancellationToken = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int delay;
                    try
                    {
                        if (!_authenticationDone)
                        {
                            _logger.LogInformation("Attempting to authenticate the session");
                            var credentials = await _localCredentialsProvider.GetCredentialsAsync<UsernamePasswordCredentialsViewModel>("CoreConnectionCredentials");
                            delay = Convert.ToInt32(await _authenticator.AuthenticateAsync(credentials.Username, credentials.Password));
                            _authenticationDone = true;
                        }
                        else
                        {
                            _logger.LogInformation("Attempting to refresh the session");
                            delay = Convert.ToInt32(await _authenticator.RefreshAsync());
                        }
                        _logger.LogDebug("Successful. Refreshing in {0} seconds", delay);
                    }
                    catch (HttpRequestException ex)
                    {
                        delay = 10;
                        _logger.LogError("Failed to authenticate. Retrying in {0} seconds", delay);
                    }

                    await Task.Delay(delay * 1000, cancellationToken);
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}