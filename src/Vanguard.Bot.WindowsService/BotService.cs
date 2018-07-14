using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vanguard.Bot.Abstractions;
using Vanguard.Bot.Discord;

namespace Vanguard.Bot.WindowsService
{
    public class BotService : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private List<IBot> _bots;

        public BotService(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<BotService>();
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing");
            _bots = new List<IBot>();

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => _cancellationTokenSource.Cancel());
            }

            if (_configuration.GetSection("Discord").Exists())
            {
                _logger.LogDebug("Initializing bot logic for Discord");
                var discordConfig = new DiscordBotConfig();
                _configuration.GetSection("Discord").Bind(discordConfig);
                var discordBot = new DiscordBot(_loggerFactory, new DiscordSocketClient(), discordConfig);
                _bots.Add(discordBot);
            }

            _logger.LogInformation("Starting {0} bots", _bots.Count);
            _bots.ForEach(t => t.RunAsync(_cancellationTokenSource.Token));
            await Task.Delay(-1, _cancellationTokenSource.Token);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}