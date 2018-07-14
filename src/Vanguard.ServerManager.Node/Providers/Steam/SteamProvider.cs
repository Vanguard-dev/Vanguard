using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vanguard.ServerManager.Node.Abstractions;

namespace Vanguard.ServerManager.Node.Providers
{
    public class SteamProvider : IGameProvider<SteamProviderOptions, SteamInstanceOptions>
    {
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly IServiceManager _serviceManager;

        public SteamProvider(ICredentialsProvider credentialsProvider, IServiceManager serviceManager)
        {
            _credentialsProvider = credentialsProvider;
            _serviceManager = serviceManager;
        }

        public async Task ProvisionServerAsync(SteamProviderOptions providerOptions, SteamInstanceOptions instanceOptions, CancellationToken cancellationToken = default)
        {
            var serviceInstalled = await _serviceManager.IsInstalledAsync(instanceOptions.Name);
            if (serviceInstalled && await _serviceManager.IsRunningAsync(instanceOptions.Name))
            {
                await _serviceManager.StopServiceAsync(instanceOptions.Name);
            }

            // Install or update the server game
            await ExecuteSteamCmdAsync(
                providerOptions,
                new[]
                {
                    $"+login {await GetCredentialsOrDefault(instanceOptions.SteamCredentialId)}",
                    $"+force_install_dir {providerOptions.InstallationRoot}\\{instanceOptions.Name}",
                    $"+app_update {instanceOptions.GameId} validate",
                    "+quit"
                }, 
                cancellationToken
            );
            // TODO: Run post update callbacks based on gameId

            // Install or update the server mods
            await ExecuteSteamCmdAsync(
                providerOptions,
                new[]
                {
                    $"+login {await GetCredentialsOrDefault(instanceOptions.SteamCredentialId)}",
                    $"+force_install_dir {providerOptions.InstallationRoot}\\{instanceOptions.Name}",
                    string.Join(' ', instanceOptions.Mods.Select(t => $"+workshop_download_item {instanceOptions.GameId} {t} validate")),
                    "+quit"
                },
                cancellationToken
            );
            // TODO: Run post mod update callbacks based on gameId

            // Install or update the server service
            var newServiceDefinition = new ServerInstanceDefinition
            {
                Name = instanceOptions.Name,
                Description = instanceOptions.Description,
                ServiceCredentialsId = instanceOptions.ServiceCredentialsId
            };
            if (!serviceInstalled)
            {
                await _serviceManager.InstallServiceAsync(newServiceDefinition);
            }
            else
            {
                await _serviceManager.UpdateServiceAsync(newServiceDefinition);
            }

            await _serviceManager.StartServiceAsync(instanceOptions.Name);
        }

        private async Task<string> GetCredentialsOrDefault(string credentialsId)
        {
            if (credentialsId == null)
            {
                return "anonymous";
            }

            var credentials = await _credentialsProvider.GetCredentialsAsync<UsernamePasswordCredentials>(credentialsId);
            if (credentials == null)
            {
                throw new Exception("Failed to retrieve steam credentials for provisioning");
            }

            return $"{credentials.Username} {credentials.Password}";
        }

        private Task ExecuteSteamCmdAsync(SteamProviderOptions providerOptions, IEnumerable<string> arguments, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = providerOptions.ExecutablePath,
                        Arguments = string.Join(' ', arguments),
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                process.Start();
                process.WaitForExit();
            }, cancellationToken);
        }
    }
}