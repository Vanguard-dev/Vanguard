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

        public void Run()
        {
            RunAsync(new CancellationToken()).GetAwaiter().GetResult();
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Start and run bots here
            throw new NotImplementedException();
        }
    }
}