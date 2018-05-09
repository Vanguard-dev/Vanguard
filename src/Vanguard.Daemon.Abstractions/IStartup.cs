using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vanguard.Daemon.Abstractions
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
        void ConfigureApp(IServiceProvider services, IConfiguration configuration);
    }
}