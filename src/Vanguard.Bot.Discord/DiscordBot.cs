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
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += OnLogMessage;
            _client.Ready += OnReady;

            _configuration = configuration;

            if (_configuration.RulesAgreement != null)
            {
                async Task OnRulesAgreed(SocketMessage message)
                {
                    if (_configuration.RulesAgreement.ListenChannels.Any(t => string.Equals(t, message.Channel.Name, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var command = ParseCommand(message);
                        if (command == null || command.Name != _configuration.RulesAgreement.Trigger)
                        {
                            return;
                        }

                        var guildUser = message.Author as SocketGuildUser;
                        var guildRole = guildUser?.Guild.Roles.FirstOrDefault(role => string.Equals(role.Name, _configuration.RulesAgreement.Role, StringComparison.CurrentCultureIgnoreCase));

                        if (guildRole == null)
                        {
                            _logger.LogCritical($"Unable to assign user a role after agreeing to rules. Missing role \"{_configuration.RulesAgreement.Role}\"");
                            await message.Channel.SendMessageAsync("Oh dear, it seems like I've been misconfigured. Please tell an administrator to check my configuration.");
                            return;
                        }

                        if (guildUser.Roles.All(t => t.Name != _configuration.RulesAgreement.Role))
                        {
                            await guildUser.AddRoleAsync(guildRole);
                            await guildUser.SendMessageAsync(_configuration.RulesAgreement.Message);
                        }

                        await message.DeleteAsync();
                    }
                }

                _client.MessageReceived += OnRulesAgreed;
            }

            if (_configuration.InfoCommands != null && _configuration.InfoCommands.Count > 0)
            {
                foreach (var infoCommand in _configuration.InfoCommands)
                {
                    async Task OnInfoCommandReceived(SocketMessage message)
                    {
                        if (infoCommand.ListenChannels.Any(t => string.Equals(t, message.Channel.Name, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            var command = ParseCommand(message);
                            if (command == null || command.Name != infoCommand.Trigger)
                            {
                                return;
                            }

                            var infoKeyword = command.Arguments.First();
                            var infoText = infoCommand.Infos.FirstOrDefault(t => t.Keywords.Contains(infoKeyword.ToLower()));
                            if (infoText != null)
                            {
                                await message.Author.SendMessageAsync(infoText.Description);
                            }
                            else
                            {
                                await message.Author.SendMessageAsync($"No info found for {infoKeyword}");
                            }

                            await message.DeleteAsync();
                        }
                    }

                    _client.MessageReceived += OnInfoCommandReceived;
                }
            }

            if (_configuration.SelfAssignRole != null)
            {
                async Task OnSelfAssignRoleReceived(SocketMessage message)
                {
                    if (_configuration.SelfAssignRole.ListenChannels.Any(t => string.Equals(t, message.Channel.Name, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var command = ParseCommand(message);
                        if (command == null)
                        {
                            return;
                        }

                        if (_configuration.SelfAssignRole.AllowedSelfAssignRoles.Any(t => string.Equals(t, command.Name, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            var guildUser = message.Author as SocketGuildUser;
                            var guildRole = guildUser?.Guild.Roles.FirstOrDefault(role => string.Equals(role.Name, command.Name, StringComparison.CurrentCultureIgnoreCase));

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
                        else
                        {
                            if (_configuration.RulesAgreement == null || !string.Equals(_configuration.RulesAgreement.Role, command.Name, StringComparison.CurrentCultureIgnoreCase))
                            {
                                await message.Author.SendMessageAsync($"{command.Name} is not a valid or allowed self-assign role. Please contact the administrators or moderators if you need help assigning your own roles.");                                
                            }
                        }

                        await message.DeleteAsync();
                    }
                }

                _client.MessageReceived += OnSelfAssignRoleReceived;
            }
        }

        private DiscordCommand ParseCommand(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
            {
                return null;
            }

            var messageContent = message.Content.ToLower();
            return !messageContent.StartsWith("!") ? null : new DiscordCommand(messageContent);
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

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            await _client.LoginAsync(TokenType.Bot, _configuration.ApiToken);
            await _client.StartAsync();
            await Task.Delay(-1, cancellationToken);
        }
    }
}