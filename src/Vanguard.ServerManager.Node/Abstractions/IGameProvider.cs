using System.Threading;
using System.Threading.Tasks;

namespace Vanguard.ServerManager.Node.Abstractions
{
    public interface IGameProvider<in TProviderOptions, in TInstanceOptions>
    {
        Task ProvisionServerAsync(TProviderOptions providerOptions, TInstanceOptions instanceOptions, CancellationToken cancellationToken = default);
    }
}