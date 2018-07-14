using System.Threading.Tasks;

namespace Vanguard.ServerManager.Node.Abstractions
{
    public interface IServiceManager
    {
        Task<bool> IsInstalledAsync(string name);
        Task<bool> IsRunningAsync(string name);
        Task InstallServiceAsync(ServerInstanceDefinition definition);
        Task UpdateServiceAsync(ServerInstanceDefinition definition);
        Task UninstallServiceAsync(ServerInstanceDefinition definition);
        Task StartServiceAsync(string name);
        Task StopServiceAsync(string name);
    }
}