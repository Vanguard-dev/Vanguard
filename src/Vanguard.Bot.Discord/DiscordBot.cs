using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vanguard.Bot.Abstractions;

namespace Vanguard.Bot.Discord
{
    public class DiscordBot : IBot
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly List<string> _allowedSelfAssignRoles;

        public DiscordBot(ILoggerFactory loggerFactory, DiscordSocketClient client, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<DiscordBot>();
            _client = client;
            _client.Log += OnLogMessage;
            _client.Ready += OnReady;
            _client.MessageReceived += OnMessageReceived;
            configuration.GetSection("Discord").Bind("AllowedSelfAssignRoles", _allowedSelfAssignRoles);
        }

        private Task OnLogMessage(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(arg.Exception, arg.Message);
                    break;
                default:
                    _logger.LogError("Discord Client sent a log event of unknown LogSeverity");
                    break;
            }

            return Task.CompletedTask;
        }

        private Task OnReady()
        {
            _logger.LogInformation("Discord bot is connected as user {0}", _client.CurrentUser);

            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
            {
                return;
            }

            // TODO: Check if role exists
            // TODO: Check if role is in allowed self assign roles
            // TODO: Superset check, ensure role is not an administrator
            // TODO: Toggle role on user
        }

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Start a loop that loops till it's cancelled or a critical error occurs (faulty api auth for example)
            throw new NotImplementedException();
        }
    }
}