using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Vanguard.Windows.InstanceManager
{
    public class ServiceHost : IDisposable
    {
        private Process _process;

        public string ServiceName { get; set; }
        public ServiceDefinition ServiceDefinition { get; set; }

        public ServiceHost(string serviceName, ServiceDefinition serviceDefinition, ILoggerFactory loggerFactory)
        {
            ServiceName = serviceName;
            ServiceDefinition = serviceDefinition;
            // TODO: Logging
        }

        public void Start()
        {
            var credentials = GetCredentials();
            _process = ProcessLauncher.RunProcess("", ServiceDefinition.UserName, ServiceDefinition.Password, ServiceDefinition.WorkingDirectory, ServiceDefinition.Executable, ServiceDefinition.Arguments);
            
            
            /*var startInfo = new ProcessStartInfo(Path.Combine(ServiceDefinition.WorkingDirectory, ServiceDefinition.Executable), ServiceDefinition.Arguments)
            {
                WorkingDirectory = ServiceDefinition.WorkingDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UserName = credentials.Item1,
                PasswordInClearText = credentials.Item2,
                LoadUserProfile = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };
            _process = Process.Start(startInfo);*/
        }

        public void Kill()
        {
            if (!_process.HasExited)
            {
                _process?.Kill();
            }
        }

        private Tuple<string, string> GetCredentials()
        {
            // TODO: Ensure exists
            // TODO: Ensure password is correct
            return new Tuple<string, string>(ServiceDefinition.UserName, ServiceDefinition.Password);
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}