using System.Threading;
using System.Threading.Tasks;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.ServerManager.Node
{
    public class Daemon : IDaemon
    {
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Start watchdog thread for game servers
            // TODO: Start MQ listener for tasks

            await Task.Delay(-1, cancellationToken);
        }
    }
}