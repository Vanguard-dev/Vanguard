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
            var serializedValue = value != null ? JsonConvert.SerializeObject(value) : null;
            return cache.SetStringAsync(key, serializedValue, cancellationToken);
        }

        public static async Task<T> GetObjectAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
        {
            var serializedValue = await cache.GetStringAsync(key, cancellationToken);
            return serializedValue != null ? JsonConvert.DeserializeObject<T>(serializedValue) : default;
        }

        public static async Task<T> GetObjectAsync<T>(this IDistributedCache cache, string key, T defaultValue, CancellationToken cancellationToken = default)
        {
            var serializedValue = await cache.GetStringAsync(key, cancellationToken);
            return serializedValue != null ? JsonConvert.DeserializeObject<T>(serializedValue) : defaultValue;
        }
    }
}