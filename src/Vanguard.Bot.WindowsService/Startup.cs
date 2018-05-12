using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.Bot.WindowsService
{
    public class Startup : IStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // TODO: Add bots and their dependencies here. Look up on when you need transient, scoped or singleton service here: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1#service-lifetimes-and-registration-options
        }

        public void ConfigureApp(IServiceProvider services, IConfiguration configuration)
        {
            // TODO: If you need runtime configuration that cannot be done during the service definition phase, you can do it here
        }
    }
}