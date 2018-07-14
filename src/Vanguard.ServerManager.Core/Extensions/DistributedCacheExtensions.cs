using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Vanguard.ServerManager.Core.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static Task SetObjectAsync(this IDistributedCache cache, string key, object value, CancellationToken cancellationToken = default)
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            return cache.SetStringAsync(key, serializedValue, cancellationToken);
        }

        public static async Task<T> GetObjectAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
        {
            var serializedValue = await cache.GetStringAsync(key, cancellationToken);
            return JsonConvert.DeserializeObject<T>(serializedValue);
        }
    }
}