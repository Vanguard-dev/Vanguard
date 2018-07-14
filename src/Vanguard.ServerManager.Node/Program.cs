using System;
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
using Newtonsoft.Json;
using Vanguard.Daemon.Abstractions;
using Vanguard.Daemon.Windows;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Node.Abstractions;

namespace Vanguard.ServerManager.Node
{
    class Program
    {
        static void Main(string[] args)
        {
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
                        // TODO: Check if registration is already done

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
                            authResponse.EnsureSuccessStatusCode();
                            var bearerToken = JsonConvert.DeserializeObject<dynamic>(await authResponse.Content.ReadAsStringAsync()).access_token.Value;

                            var registrationResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"{apiRoot}/api/node")
                            {
                                Method = HttpMethod.Post,
                                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", bearerToken) },
                                Content = new StringContent(JsonConvert.SerializeObject(new ServerNodeViewModel
                                {
                                    Name = nodeName,
                                    PublicKey = certificate.GetPublicKeyString()
                                }))
                            });
                            registrationResponse.EnsureSuccessStatusCode();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }
                });
            });

            app.Command("start", command =>
            {
                var foregroundSwitch = command.Option("-f|--foreground", "Run the service in foreground mode", CommandOptionType.NoValue);

                command.OnExecute(async () =>
                {
                    var daemonHost = new DaemonHost()
                        .UseService<Daemon>();

                    if (foregroundSwitch.HasValue())
                    {
                        try
                        {
                            var cancellationTokenSource = new CancellationTokenSource();
                            var workTask = daemonHost.RunAsync(cancellationTokenSource.Token);
                            Console.ReadKey();
                            cancellationTokenSource.Cancel();
                            await workTask;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            daemonHost.RunAsService();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
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
