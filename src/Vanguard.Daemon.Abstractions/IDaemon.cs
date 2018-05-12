using System.Threading;
using System.Threading.Tasks;

namespace Vanguard.Daemon.Abstractions
{
    public interface IDaemon
    {
        Task RunAsync(CancellationToken cancellationToken = default);
        void Kill();
    }
}