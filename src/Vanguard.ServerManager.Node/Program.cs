using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Vanguard.Extensions.Hosting;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Node.Abstractions;
using Vanguard.ServerManager.Node.Abstractions.Windows;
using Vanguard.ServerManager.Node.Core;

namespace Vanguard.ServerManager.Node
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment}.json", true, true)
                        .AddEnvironmentVariables("VANGUARD_")
                        .AddCommandLine(args)
                        .Build();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    NodeOptions nodeOptions;
                    if ((string) RegistryHelper.GetVanguardKey().GetValue("NodeInstalled") == "yes")
                    {
                        nodeOptions = new NodeOptions
                        {
                            IsInstalled = true,
                            NodeName = RegistryHelper.GetVanguardKey().GetValue("NodeName") as string,
                            CoreConnectionHostname = RegistryHelper.GetVanguardKey().GetValue("CoreConnectionHostname") as string,
                            CoreConnectionNoSsl = (string) RegistryHelper.GetVanguardKey().GetValue("CoreConnectionNoSsl") == "yes",
                            CoreConnectionIgnoreSslWarnings = (string) RegistryHelper.GetVanguardKey().GetValue("CoreConnectionIgnoreSslWarnings") == "yes"
                        };
                    }
                    else
                    {
                        nodeOptions = new NodeOptions();
                    }
                    services.AddSingleton(nodeOptions);

                    services.AddSingleton<LocalCredentialsProvider>();
                    services.AddSingleton<Authenticator>();
                    services.AddTransient<NodeHttpClient>();
                    services.AddSingleton<IHostedService, AuthService>();
                    services.AddSingleton<IHostedService, NodeService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseConsoleLifetime()
                .Build();

            var logger = host.Services.GetService<ILoggerFactory>().CreateLogger("Main");
            var hostOptions = host.Services.GetService<NodeOptions>();

            var app = new CommandLineApplication { Name = "Vanguard Server Manager Node" };
            app.HelpOption("-?|-h|--help", true);

            app.Command("install", command =>
            {
                var nodeNameOption = command.Option<string>("-n|--name", "Name of the server manager node", CommandOptionType.SingleValue);
                var coreHostnameOption = command.Option<string>("-a|--address", "Hostname or IP address of the server manager core server", CommandOptionType.SingleValue);
                var certificateFile = command.Option<string>("-c|--certificate", "Provide a certificate instead of generating a self-signed one", CommandOptionType.SingleValue);
                var certificateKeyFile = command.Option<string>("-k|--key-file", "Provide a certificate key file instead of prompting for password for the provided certificate file", CommandOptionType.SingleValue);
                var useHttp = command.Option<bool>("--no-ssl", "Don't use HTTPS for core communication", CommandOptionType.NoValue);
                var useInsecure = command.Option<bool>("--insecure", "Ignore SSL warnings and errors", CommandOptionType.NoValue);
                
                command.OnExecute(async () =>
                {
                    try
                    {
                        if (hostOptions.IsInstalled)
                        {
                            logger.LogInformation("The service has already been installed");
                            return 0;
                        }

                        var nodeName = nodeNameOption.HasValue() ? nodeNameOption.ParsedValue : Prompt.GetString("Name of the server manager node (Must be unique):");
                        var coreHostname = coreHostnameOption.HasValue() ? coreHostnameOption.ParsedValue : Prompt.GetString("Hostname or IP address of the server manager core server:");
                        var coreUsername = Prompt.GetString("Username for the registration:");
                        var corePassword = Prompt.GetPassword("Password for the registration:");

                        X509Certificate2 certificate;
                        if (certificateFile.HasValue())
                        {
                            certificate = EncryptionHelpers.LoadServerNodeCertificate(certificateFile.ParsedValue, certificateKeyFile.ParsedValue);
                        }
                        else
                        {
                            // TODO: Check for generated certificate and load it

                            var password = Prompt.GetPassword("Provide the certificate password:");
                            var confirmPassword = Prompt.GetPassword("Confirm the certificate password:");
                            if (password != confirmPassword)
                            {
                                throw new Exception("Passwords don't match");
                            }

                            certificate = EncryptionHelpers.GenerateServerNodeCertificate(nodeName, password);
                        }

                        var nodeOptions = new NodeOptions
                        {
                            CoreConnectionHostname = coreHostname,
                            CoreConnectionNoSsl = useHttp.HasValue(),
                            CoreConnectionIgnoreSslWarnings = useInsecure.HasValue()
                        };
                        var authenticator = new Authenticator(nodeOptions);
                        try
                        {
                            await authenticator.AuthenticateAsync(coreUsername, corePassword);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("Failed to authenticate for installation: {0}", ex);
                            return 1;
                        }

                        using (var client = new NodeHttpClient(authenticator, nodeOptions))
                        {
                            var registrationPayload = JsonConvert.SerializeObject(new ServerNodeViewModel
                            {
                                Name = nodeName,
                                PublicKey = certificate.GetPublicKeyString()
                            }, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                            logger.LogInformation("Starting node registration");
                            logger.LogDebug("Attempting to register the node");
                            logger.LogTrace(registrationPayload);
                            var registrationResponse = await client.PostAsync($"{nodeOptions.ApiRoot}/api/node", new StringContent(registrationPayload, Encoding.Default, "application/json"));
                            if (!registrationResponse.IsSuccessStatusCode)
                            {
                                logger.LogError("Failed to register the node: [{0}] {1}", registrationResponse.StatusCode, await registrationResponse.Content.ReadAsStringAsync());
                                return 1;
                            }

                            var credentials = JsonConvert.DeserializeObject<UsernamePasswordCredentialsViewModel>(await registrationResponse.Content.ReadAsStringAsync());
                            var localCredentialsProvider = host.Services.GetService<LocalCredentialsProvider>();
                            try
                            {
                                await localCredentialsProvider.SetCredentialsAsync("CoreConnectionCredentials", credentials);
                            }
                            catch (CredentialProviderException ex)
                            {
                                // TODO: Implement recovery action via option flag to generate a new password
                                logger.LogError("Failed to store retrieved credentials: {0}", ex);
                                return 1;
                            }

                            RegistryHelper.GetVanguardKey().SetValue("NodeInstalled", "yes");
                            RegistryHelper.GetVanguardKey().SetValue("NodeName", nodeName);
                            RegistryHelper.GetVanguardKey().SetValue("CoreConnectionHostname", coreHostname);
                            RegistryHelper.GetVanguardKey().SetValue("CoreConnectionNoSsl", useHttp.HasValue() ? "yes" : "no");
                            RegistryHelper.GetVanguardKey().SetValue("CoreConnectionIgnoreSslWarnings", useInsecure.HasValue() ? "yes" : "no");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Install failed due to an unexpected exception: {0}", ex);
                        throw;
                    }

                    return 0;
                });
            });

            app.Command("start", command =>
            {
                var foregroundSwitch = command.Option("-f|--foreground", "Run the service in foreground mode", CommandOptionType.NoValue);

                command.OnExecute(async () =>
                {
                    using (host)
                    {
                        var nodeOptions = host.Services.GetService<NodeOptions>();
                        if (!nodeOptions.IsInstalled)
                        {
                            logger.LogInformation("The service has not been installed yet");
                            return 1;
                        }

                        try
                        {
                            if (foregroundSwitch.HasValue())
                            {
                                // TODO: Remove when child process kill has been fixed. See https://github.com/dotnet/cli/issues/7426
                                var cancellationTokenSource = new CancellationTokenSource();
                                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                                {
                                    cancellationTokenSource.Cancel();
                                };
                                Console.CancelKeyPress += (sender, e) =>
                                {
                                    e.Cancel = true;
                                    cancellationTokenSource.Cancel();
                                };

                                await host.StartAsync(cancellationTokenSource.Token);
                                await host.WaitForShutdownAsync(cancellationTokenSource.Token);
                                //await host.StartAsync();
                                //await host.WaitForShutdownAsync();
                            }
                            else
                            {
                                host.RunAsService();
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            return 0;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("Start failed due to an unexpected exception: {0}", ex);
                            return 1;
                        }

                        return 0;
                    }
                });
            });

            if (args.Length == 0)
            {
                app.ShowHint();
            }
            else
            {
                app.Execute(args);
            }
        }
    }
}
