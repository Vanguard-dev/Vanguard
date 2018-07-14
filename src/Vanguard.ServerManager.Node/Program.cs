﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Vanguard.Daemon.Abstractions;
using Vanguard.Daemon.Windows;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Node.Abstractions;
using Vanguard.ServerManager.Node.Abstractions.Windows;

namespace Vanguard.ServerManager.Node
{
    class Program
    {
        static void Main(string[] args)
        {
            var daemonHost = new DaemonHost()
                .UseStartup<Startup>()
                .UseService<Daemon>();

            var logger = daemonHost.Services.GetService<ILoggerFactory>().CreateLogger("Main");

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
                        if ((string)RegistryHelper.GetVanguardKey().GetValue("NodeInstalled") == "yes")
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

                        using (var client = new HttpClient(new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => useInsecure.HasValue() || new HttpClientHandler().ServerCertificateCustomValidationCallback(message, cert, chain, errors)
                        }))
                        {
                            var apiRoot = $"{(useHttp.HasValue() ? "http" : "https")}://{coreHostname}";
                            var authResponse = await client.PostAsync($"{apiRoot}/connect/token", new StringContent($"username={WebUtility.UrlEncode(coreUsername)}&password={WebUtility.UrlEncode(corePassword)}&grant_type=password&scope=openid+offline_access", Encoding.Default, "application/x-www-form-urlencoded"));
                            if (!authResponse.IsSuccessStatusCode)
                            {
                                logger.LogError("Failed to register the node: [{0}] {1}", authResponse.StatusCode, await authResponse.Content.ReadAsStringAsync());
                                return 1;
                            }
                            var bearerToken = JsonConvert.DeserializeObject<dynamic>(await authResponse.Content.ReadAsStringAsync()).access_token.Value;

                            var registrationPayload = JsonConvert.SerializeObject(new ServerNodeViewModel
                            {
                                Name = nodeName,
                                PublicKey = certificate.GetPublicKeyString()
                            }, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                            logger.LogDebug("Attempting to register the node. Payload: {0}", registrationPayload);
                            var registrationResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"{apiRoot}/api/node")
                            {
                                Method = HttpMethod.Post,
                                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", bearerToken) },
                                Content = new StringContent(registrationPayload, Encoding.Default, "application/json")
                            });
                            if (!registrationResponse.IsSuccessStatusCode)
                            {
                                logger.LogError("Failed to register the node: [{0}] {1}", registrationResponse.StatusCode, await registrationResponse.Content.ReadAsStringAsync());
                                return 1;
                            }

                            var credentials = JsonConvert.DeserializeObject<UsernamePasswordCredentialsViewModel>(await registrationResponse.Content.ReadAsStringAsync());
                            var localCredentialsProvider = daemonHost.Services.GetService<LocalCredentialsProvider>();
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
                    var nodeOptions = daemonHost.Services.GetService<NodeOptions>();
                    if (!nodeOptions.IsInstalled)
                    {
                        logger.LogInformation("The service has already not been installed yet");
                        return 1;
                    }

                    try
                    {
                        if (foregroundSwitch.HasValue())
                        {
                            var cancellationTokenSource = new CancellationTokenSource();
                            var workTask = daemonHost.RunAsync(cancellationTokenSource.Token);
                            Console.ReadKey();
                            cancellationTokenSource.Cancel();
                            await workTask;
                        }
                        else
                        {
                            daemonHost.RunAsService();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Start failed due to an unexpected exception: {0}", ex);
                        return 1;
                    }

                    return 0;
                });
            });

            if (args.Length == 0)
            {
                app.ShowHint();
            }
            else
            {
                app.Execute(args);
                Console.ReadKey();
            }
        }
    }
}
