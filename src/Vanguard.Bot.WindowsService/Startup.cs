using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vanguard.Bot.Discord;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Bot.WindowsService
{
    public class Startup : IStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public void ConfigureApp(IServiceProvider services, IConfiguration configuration)
        {
        }
    }
}