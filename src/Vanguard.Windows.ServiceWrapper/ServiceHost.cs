using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Vanguard.Windows.ServiceWrapper
{
    public class ServiceHost : IDisposable
    {
        private ILogger _logger;
        private Process _process;

        public event EventHandler Exited;
        public bool IsRunning => !_process?.HasExited ?? false;
        public ServiceDefinition ServiceDefinition { get; set; }

        public ServiceHost(ServiceDefinition serviceDefinition, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ServiceHost>();
            ServiceDefinition = serviceDefinition;
        }

        public void Start()
        {
            var startInfo = new ProcessStartInfo(Path.Combine(ServiceDefinition.WorkingDirectory, ServiceDefinition.Executable), ServiceDefinition.Arguments)
            {
                WorkingDirectory = ServiceDefinition.WorkingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            _process = Process.Start(startInfo);
            if (_process == null)
            {
                OnProcessExit(null, null);
            }

            _process.EnableRaisingEvents = true;
            _process.Exited += OnProcessExit;
        }

        public void Kill()
        {
            if (IsRunning)
            {
                _process?.Kill();
            }
        }

        public void Stop()
        {
            _process?.Close();
            if (IsRunning)
            {
                _process?.WaitForExit(2500);
                Kill();
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            Exited?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}