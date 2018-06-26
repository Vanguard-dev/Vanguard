using System.Threading;
using System.Threading.Tasks;
using Vanguard.Daemon.Abstractions;

namespace Vanguard.ServerManager.Host
{
    public class Daemon : IDaemon
    {
        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public void Kill()
        {
            throw new System.NotImplementedException();
        }
    }
}