using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Vanguard.Bot.Abstractions;

namespace Vanguard.Bot.Discord
{
    public class DiscordBot : IBot
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly DiscordBotConfig _configuration;

        public DiscordBot(ILoggerFactory loggerFactory, DiscordSocketClient client, DiscordBotConfig configuration)
        {
            _logger = loggerFactory.CreateLogger<DiscordBot>();
            _client = client;
            _client.Log += OnLogMessage;
            _client.Ready += OnReady;
            _client.MessageReceived += OnMessageReceived;

            _configuration = configuration;
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

            var messageContent = message.Content.ToLower();
            if (!messageContent.StartsWith("!"))
            {
                return;
            }

            if (messageContent.StartsWith("!serverinfo"))
            {
                var serverKeyword = messageContent.Split(' ')[1];
                var serverInfo = _configuration.ServerList.FirstOrDefault(t => t.Keywords.Contains(serverKeyword.ToLower()));
                if (serverInfo != null)
                {
                    await message.Author.SendMessageAsync(serverInfo.Description);
                }
                else
                {
                    await message.Author.SendMessageAsync($"No server info found for {serverKeyword}");
                }
            }
            else
            {
                var roleName = messageContent.Substring(1);
                if (_configuration.AllowedSelfAssignRoles.Any(t => string.Equals(t, roleName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    ToggleRole(message, roleName);
                }
                else
                {
                    await message.Author.SendMessageAsync($"{roleName} is not a valid or allowed self-assign role. Please contact the administrators or moderators if you need help assigning your own roles.");
                }
            }

            if (message.Channel.Name == _configuration.SelfAssignChannel)
            {
                await message.DeleteAsync();
            }
        }

        private async void ToggleRole(SocketMessage message, string roleName)
        {
            var guildUser = message.Author as SocketGuildUser;
            var guildRole = guildUser?.Guild.Roles.FirstOrDefault(role => role.Name.ToLower() == roleName);

            if (guildRole == null)
            {
                return;
            }

            // Remove role
            if (guildUser.Roles.Any(role => role.Name == guildRole.Name))
            {
                await guildUser.RemoveRoleAsync(guildRole);
                await guildUser.SendMessageAsync($"Role {guildRole.Name} removed!");
            }
            // Add role
            else
            {
                await guildUser.AddRoleAsync(guildRole);
                await guildUser.SendMessageAsync($"Role {guildRole.Name} added!");
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            await _client.LoginAsync(TokenType.Bot, _configuration.ApiToken);
            await _client.StartAsync();
            await Task.Delay(-1, cancellationToken);
        }
    }
}