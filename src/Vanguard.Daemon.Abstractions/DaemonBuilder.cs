using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Vanguard.Daemon.Abstractions
{
    public class DaemonBuilder
    {
        private readonly ServiceCollection _serviceCollection;
        private readonly IConfigurationRoot _configuration;
        private IStartup _startup;

        public DaemonBuilder(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("VANGUARD_ENVIRONMENT") ?? "Production";
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables("VANGUARD_")
                .AddCommandLine(args)
                .Build();

            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddSingleton<IConfiguration>(_configuration);

            ConfigureLogging(logging =>
            {
                logging.AddConfiguration(_configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            });
        }

        public DaemonBuilder UseStartup<TStartup>() where TStartup : class, IStartup, new()
        {
            _startup = new TStartup();
            _startup.ConfigureServices(_serviceCollection, _configuration);

            return this;
        }

        public DaemonBuilder UseService<TService>() where TService : class, IDaemon
        {
            _serviceCollection.AddSingleton<IDaemon, TService>();

            return this;
        }

        public DaemonBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging)
        {
            _serviceCollection.AddLogging(builder =>
            {
                builder.AddConfiguration(_configuration.GetSection("Logging"));
                configureLogging(builder);
            });
            return this;
        }

        public IDaemon Build()
        {
            // Ensure mandatory services are configured
            if (_serviceCollection.All(t => t.ServiceType != typeof(IDaemon)))
            {
                throw new EntryPointNotFoundException("You need to provide the required IDaemon implementation for the service logic");
            }

            var services = _serviceCollection.BuildServiceProvider();
            _startup?.ConfigureApp(services, _configuration);
            return services.GetRequiredService<IDaemon>();
        }
    }
}