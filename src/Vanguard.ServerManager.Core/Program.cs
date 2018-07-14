using System;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vanguard.ServerManager.Core.Entities;

namespace Vanguard.ServerManager.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();
            var serviceScope = webHost.Services.CreateScope();
            var app = new CommandLineApplication { Name = "servermanager-core" };
            app.HelpOption("-?|-h|--help", true);

            app.Command("start", command => { command.OnExecute(() => webHost.Run()); });

            app.Command("migrate", command =>
            {
                command.Description = "Apply database migrations";

                command.OnExecute(async () =>
                {
                    var dbContext = serviceScope.ServiceProvider.GetService<VanguardDbContext>();
                    await dbContext.Database.MigrateAsync();
                }); 
            });

            app.Command("createsuperuser", command =>
            {
                command.Description = "Create a new super user to the system";

                var emailArgument = command.Argument("email", "UserName address for the new super user.");
                var providedPasswordOption = command.Option("-s|--set-password", "Set user password via argument.", CommandOptionType.SingleValue);

                command.OnExecute(async () =>
                {
                    // TODO: Audit logging
                    var user = new VanguardUser
                    {
                        UserName = emailArgument.Value,
                        Email = emailArgument.Value
                    };

                    string password;
                    if (providedPasswordOption.HasValue())
                    {
                        password = providedPasswordOption.Value();
                    }
                    else
                    {
                        password = Prompt.GetPassword("Provide the user password:");
                        var confirmPassword = Prompt.GetPassword("Confirm the user password:");
                        if (password != confirmPassword)
                        {
                            throw new Exception("Passwords don't match");
                        }
                    }

                    var userManager = serviceScope.ServiceProvider.GetService<UserManager<VanguardUser>>();
                    var result = await userManager.CreateAsync(user, password);
                    if (!result.Succeeded)
                    {
                        Console.WriteLine(string.Join('\n', result.Errors.Select(t => $"[{t.Code}] {t.Description}")));
                        return 1;
                    }

                    foreach (var fieldInfo in typeof(RoleConstants).GetFields())
                    {
                        result = await userManager.AddToRoleAsync(user, fieldInfo.Name);
                        if (!result.Succeeded)
                        {
                            Console.WriteLine(string.Join('\n', result.Errors.Select(t => $"[{t.Code}] {t.Description}")));
                            return 1;
                        }
                    }

                    return 0;
                });

                if (args.Length == 0)
                {
                    app.ShowHint();
                }
                else
                {
                    app.Execute(args);
                }
            });
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
