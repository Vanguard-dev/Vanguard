using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Bot.WindowsService
{
    public class BotManager : IDaemon
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public BotManager(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<BotManager>();
            _configuration = configuration;
        }

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Implement a bot manager logic here. The manager should start the bots and monitor their health.
            throw new NotImplementedException();
        }

        public void Kill()
        {
            // TODO: This is used to forcefully kill the daemon. Kill and dispose all the required bots here.
            throw new NotImplementedException();
        }
    }
}