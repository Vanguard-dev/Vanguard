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
        private readonly string _environmentName;

        private ServiceProvider _services;
        public ServiceProvider Services
        {
            get
            {
                if (_services == null)
                {
                    throw new InvalidOperationException("Unable to access service provider before the daemon has been built. Use Build() to build the daemon.");
                }

                return _services;
            }
        }

        public DaemonBuilder(string[] args)
        {
            _environmentName = Environment.GetEnvironmentVariable("VANGUARD_ENVIRONMENT") ?? args.FirstOrDefault(t => t.StartsWith("/Environment:"))?.Split(':').Last() ?? "Production";
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{_environmentName}.json", true, true)
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
            if (_services == null)
            {
                // Ensure mandatory services are configured
                if (_serviceCollection.All(t => t.ServiceType != typeof(IDaemon)))
                {
                    throw new EntryPointNotFoundException("You need to provide the required IDaemon implementation for the service logic");
                }

                _services = _serviceCollection.BuildServiceProvider();
                _startup?.ConfigureApp(_services, _configuration);
                _services.GetService<ILoggerFactory>().CreateLogger<DaemonBuilder>().LogInformation("Daemon starting in {0} environment", _environmentName);
            }
            else
            {
                _services.GetService<ILoggerFactory>().CreateLogger<DaemonBuilder>().LogWarning("Daemon has already been built in {0} environment. Returning the existing instance. Check your code if this is not intentional", _environmentName);
            }

            return _services.GetRequiredService<IDaemon>();
        }
    }
}