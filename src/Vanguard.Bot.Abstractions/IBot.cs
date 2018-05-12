using System.Threading;
using System.Threading.Tasks;

namespace Vanguard.Bot.Abstractions
{
    public interface IBot
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}